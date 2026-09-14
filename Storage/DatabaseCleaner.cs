using Dapper;
using Storage.Repositories.Providers;

namespace Storage
{
    /// <summary>
    /// Background service that periodically removes test meeting data from the database.
    /// Runs at 1:00 AM daily to clean up meetings marked with test identifiers.
    /// </summary>
    public class DatabaseCleaner : BackgroundService
    {
        private readonly ILogger<DatabaseCleaner> _logger;
        private readonly IDatabaseConnectionFactory _connectionFactory;

        /// <summary>
        /// Initializes a new instance of the DatabaseCleaner with required dependencies.
        /// </summary>
        /// <param name="logger">Logger for recording cleanup operations and errors.</param>
        /// <param name="connectionFactory">Factory for creating database connections.</param>
        public DatabaseCleaner(ILogger<DatabaseCleaner> logger, IDatabaseConnectionFactory connectionFactory)
        {
            _logger = logger;
            _connectionFactory = connectionFactory;
        }

        /// <summary>
        /// Main cleanup loop that runs continuously until cancellation is requested.
        /// Checks the current hour every 60 minutes and performs cleanup at 1:00 AM.
        /// Deletes meetings with names containing 'TESTIKOKOUS' or titles containing '*TESTI*'.
        /// </summary>
        /// <param name="stoppingToken">Cancellation token for shutting down the loop.</param>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            const int LoopDelayMS = 1000 * 60 * 60; // 60 minutes
            while (!stoppingToken.IsCancellationRequested)
            {
                var hours = DateTime.Now.Hour;
                _logger.LogInformation("DoCleaning {0}", hours);

                if (hours == 1)
                {
                    _logger.LogInformation("Removing test data from database.");
                    var sqlQuery = "DELETE FROM meetings WHERE name LIKE '%TESTIKOKOUS%' OR meeting_title_fi LIKE '*TESTI* %' OR meeting_title_sv LIKE '*TESTI* %'";

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

                await Task.Delay(LoopDelayMS, stoppingToken);
            }
        }
    }
}
