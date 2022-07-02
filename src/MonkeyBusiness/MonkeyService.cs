using Microsoft.Extensions.Logging;
using MonkeyCage.Models;

namespace MonkeyCage.MonkeyBusiness
{
    public class MonkeyService
    {
        private readonly ILogger<MonkeyService> _logger;
        private readonly MonkeyFactory _monkeyFactory;

        public MonkeyService(
            ILogger<MonkeyService> logger,
            MonkeyFactory monkeyFactory)
        {
            _logger = logger;
            _monkeyFactory = monkeyFactory;
        }

        public async Task<KeyHittingResult> ProcessRequest(RequestModel request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting {MonkeyCount} monkeys to find '{TargetText}' in {Timeout}.", request.MonkeyCount, request.TargetText, request.Timeout);

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(request.Timeout);

            var monkeys = Enumerable.Range(0, request.MonkeyCount)
                .Select(_ => _monkeyFactory.Create())
                .ToArray();

            _logger.LogDebug("{MonkeyCount} monkeys created.", request.MonkeyCount);

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

            cancellationToken.ThrowIfCancellationRequested();

            var results = tasks.Select(o => o.Task.Result);

            return GetBestResult(results);
        }

        private KeyHittingResult GetBestResult(IEnumerable<KeyHittingResult> results)
        {
            var best = new KeyHittingResult(string.Empty, string.Empty, 0);

            foreach (var result in results)
            {
                if (result.TextFound.Length > best.TextFound.Length)
                {
                    best = result;
                }
            }

            return best;
        }
    }
}
