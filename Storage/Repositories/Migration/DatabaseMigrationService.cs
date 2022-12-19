using Npgsql;
using Storage.Repositories.Providers;

namespace Storage.Repositories.Migration
{
    public class DatabaseMigrationService : IHostedService
    {
        private readonly IDatabaseConnectionFactory _databaseConnectionFactory;
        private readonly ILogger<DatabaseMigrationService> _logger;

        public DatabaseMigrationService(IDatabaseConnectionFactory databaseConnectionFactory,
            ILogger<DatabaseMigrationService> logger)
        {
            _databaseConnectionFactory = databaseConnectionFactory;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var connection = await _databaseConnectionFactory.CreateOpenConnection();
                var sqlScript = File.ReadAllText("./SqlScripts/CreateTables.sql");
                NpgsqlCommand command = new NpgsqlCommand(sqlScript, connection);
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to migrage database", ex);
                throw new Exception("Failed to migrage database", ex);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
