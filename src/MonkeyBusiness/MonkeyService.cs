using System.Diagnostics;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging;
using MonkeyCage.Models;

namespace MonkeyCage.MonkeyBusiness
{
    public class MonkeyService
    {
        private readonly ILogger<MonkeyService> _logger;
        private readonly TelemetryClient _telemetryClient;
        private readonly HttpClient _httpClient;
        private readonly MonkeyFactory _monkeyFactory;
        private readonly ResultPersistenceService _resultPersistenceService;

        public MonkeyService(
            ILogger<MonkeyService> logger,
            TelemetryClient telemetryClient,
            HttpClient httpClient,
            MonkeyFactory monkeyFactory,
            ResultPersistenceService resultPersistenceService)
        {
            _logger = logger;
            _telemetryClient = telemetryClient;
            this._httpClient = httpClient;
            _monkeyFactory = monkeyFactory;
            _resultPersistenceService = resultPersistenceService;
        }

        public async Task<KeyHittingResult[]> ProcessRequest(RequestModel request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.TargetText))
            {
                _logger.LogInformation("Target text is null or empty; no monkey business required.");
                return Array.Empty<KeyHittingResult>();
            }

            if (request.GetInspiration)
            {
                _logger.LogDebug("Getting inspiration from The Bard.");
                _ = await _httpClient.GetByteArrayAsync("https://stmonkeycageprod001.blob.core.windows.net/images/shakespeare.jpg", cancellationToken);
            }

            var stopWatch = new Stopwatch();

            _logger.LogInformation("Starting {MonkeyCount} monkeys to find '{TargetText}' in {Timeout}.", request.MonkeyCount, request.TargetText, request.Timeout);

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(request.Timeout);

            var monkeys = Enumerable.Range(0, request.MonkeyCount)
                .Select(_ => _monkeyFactory.Create())
                .ToArray();

            stopWatch.Start();

            var tasks = monkeys
                .Select(o => new
                {
                    Monkey = o,
                    Task = Task.Run(() => o.HitKeys(request.TargetText, cts.Token))
                })
                .ToList();

            _logger.LogDebug("{MonkeyCount} monkeys started.", request.MonkeyCount);

            var runningTasks = tasks.ToDictionary(o => o.Task, o => o.Monkey);

            while (runningTasks.Any())
            {
                var completedTask = await Task.WhenAny(runningTasks.Keys);
                var finishedBy = runningTasks[completedTask];

                _logger.LogDebug("{MonkeyName} completed.", finishedBy.Name);

                if (completedTask.Result.IsSuccess)
                {
                    cts.Cancel();
                }

                runningTasks.Remove(completedTask);
            }

            stopWatch.Stop();

            cancellationToken.ThrowIfCancellationRequested();

            var results = tasks.Select(o => o.Task.Result)
                .OrderByDescending(o => o.IsSuccess)
                .ThenByDescending(o => o.TextFound.Length)
                .ToArray();

            if (!results.Any(o => o.IsSuccess))
            {
                _logger.LogWarning(
                    "{MonkeyCount} monkeys could not find '{TargetText}' after {ElapsedTime}.",
                    request.MonkeyCount,
                    request.TargetText,
                    stopWatch.Elapsed);
            }

            if (results.Any())
            {
                _logger.LogInformation(
                    "{MonkeyCount} monkeys found '{BestFoundText}' of '{NormalizedTargetText}' with {TotalKeyPressCount} key presses after {ElapsedTime}.",
                    request.MonkeyCount,
                    results.First().TextFound,
                    request.TargetText,
                    results.Sum(o => o.KeyPresses),
                    stopWatch.Elapsed);

                _telemetryClient.TrackEvent(
                    "RequestCompleted",
                    new Dictionary<string, string>
                    {
                        { "MonkeyCount", request.MonkeyCount.ToString() },
                        { "TargetText", request.TargetText },
                        { "BestFoundText", results.First().TextFound },
                        { "TotalKeyPressCount", results.Sum(o => o.KeyPresses).ToString() },
                        { "ElapsedTime", stopWatch.Elapsed.ToString() }
                    });

                if (request.SaveToDatabase)
                {
                    _logger.LogDebug("Request requires result to be saved to database.");
                    await _resultPersistenceService.SaveResultToDatabase(results, cancellationToken);
                }
            }

            return results;
        }
    }
}
