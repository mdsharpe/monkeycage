using System.Data.SqlClient;
using Microsoft.Extensions.Logging;
using MonkeyCage.Models;

namespace MonkeyCage.MonkeyBusiness
{
    public class ResultPersistenceService
    {
        private readonly ILogger<ResultPersistenceService> _logger;

        public ResultPersistenceService(ILogger<ResultPersistenceService> logger)
        {
            _logger = logger;
        }

        public async Task SaveResultToDatabase(ICollection<KeyHittingResult> _, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Opening database connection...");

            using var connection = new SqlConnection("Data Source=missing.invalid;Connection Timeout=5");
            
            await connection.OpenAsync(cancellationToken);
        }
    }
}
