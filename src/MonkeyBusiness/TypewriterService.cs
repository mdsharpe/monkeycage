using Microsoft.Extensions.Logging;
using MonkeyCage.Models;

namespace MonkeyCage.MonkeyBusiness
{
    public class TypewriterService
    {
        private readonly ILogger<TypewriterService> _logger;

        public TypewriterService(ILogger<TypewriterService> logger)
        {
            _logger = logger;
        }

        public async Task<KeyHittingResult> Engage(RequestModel request, CancellationToken cancellationToken)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken,
                new CancellationTokenSource(request.Timeout).Token);

            var monkeys = Enumerable.Range(0, request.MonkeyCount)
                .Select(_ => new Monkey())
                .ToArray();

            var tasks = monkeys
                .Select(o => Task.Run(() => o.HitKeys(request.TargetText, cts.Token)))
                .ToList();

            var runningTasks = tasks.ToList();

            while (runningTasks.Any())
            {
                var completedTask = await Task.WhenAny(runningTasks);
                
                if (completedTask.Result.IsSuccess)
                {
                    cts.Cancel();
                }

                runningTasks.Remove(completedTask);
            }

            cancellationToken.ThrowIfCancellationRequested();

            var results = tasks.Select(o => o.Result);

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
