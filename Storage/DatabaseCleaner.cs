using Dapper;
using Storage.Repositories.Providers;

namespace Storage
{
    public class DatabaseCleaner : IHostedService
    {
        private readonly ILogger<DatabaseCleaner> _logger;
        private readonly IDatabaseConnectionFactory _connectionFactory;

        public DatabaseCleaner(ILogger<DatabaseCleaner> logger, IDatabaseConnectionFactory connectionFactory)
        {
            _logger = logger;
            _connectionFactory = connectionFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() => DoCleaningLoop(cancellationToken), cancellationToken);
        }

        private async void DoCleaningLoop(CancellationToken cancellationToken)
        {
            const int LoopDelayMS = 1000 * 60 * 60; // 60 minutes
            while (!cancellationToken.IsCancellationRequested)
            {
                var hours = DateTime.Now.Hour;
                _logger.LogInformation("DoCleaning {0}", hours);

                if (hours == 1)
                {
                    _logger.LogInformation("Removing test data from database.");
                    var sqlQuery = "DELETE FROM meetings WHERE name LIKE '%TESTIKOKOUS%'";

                    try
                    {
                        using var connection = await _connectionFactory.CreateOpenConnection();
                        await connection.ExecuteAsync(sqlQuery);
                    }
                    catch (Exception exception)
                    {
                        _logger.LogError(exception, "DoCleaning failed");
                    }
                }

                await Task.Delay(LoopDelayMS);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
