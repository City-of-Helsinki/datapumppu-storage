using Npgsql;
using Storage.Repositories.Providers;

namespace Storage.Repositories.Migration
{
    public class DatabaseMigrationService : IHostedService
    {
        private readonly IDatabaseConnectionFactory _databaseConnectionFactory;

        public DatabaseMigrationService(IDatabaseConnectionFactory databaseConnectionFactory)
        {
            _databaseConnectionFactory = databaseConnectionFactory;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var connection = await _databaseConnectionFactory.CreateOpenConnection();

            var sqlScript = File.ReadAllText("./SqlScripts/CreateTables.sql");
            NpgsqlCommand command = new NpgsqlCommand(sqlScript, connection);
            await command.ExecuteNonQueryAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
