using Npgsql;
using Storage.Repositories.Providers;

namespace Storage.Repositories.Migration
{
    /// <summary>
    /// Background service that automatically executes database migrations on application startup.
    /// Reads and executes SQL scripts from the SqlScripts directory to create or update the database schema.
    /// </summary>
    /// <remarks>
    /// This service runs synchronously during application startup to ensure the database schema is up-to-date
    /// before processing any requests. If migration fails, the application will not start.
    /// </remarks>
    public class DatabaseMigrationService : IHostedService
    {
        private readonly IDatabaseConnectionFactory _databaseConnectionFactory;
        private readonly ILogger<DatabaseMigrationService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseMigrationService"/> class.
        /// </summary>
        /// <param name="databaseConnectionFactory">Factory for creating database connections.</param>
        /// <param name="logger">Logger for recording migration progress and errors.</param>
        public DatabaseMigrationService(IDatabaseConnectionFactory databaseConnectionFactory,
            ILogger<DatabaseMigrationService> logger)
        {
            _databaseConnectionFactory = databaseConnectionFactory;
            _logger = logger;
        }

        /// <summary>
        /// Executes database migration scripts when the application starts.
        /// Reads the CreateTables.sql script and executes it against the database.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for stopping the operation.</param>
        /// <returns>A task representing the asynchronous migration operation.</returns>
        /// <exception cref="Exception">Thrown when migration fails, preventing application startup.</exception>
        /// <remarks>
        /// The SQL script in SqlScripts/CreateTables.sql should be idempotent (safe to run multiple times)
        /// using CREATE TABLE IF NOT EXISTS and similar statements.
        /// </remarks>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var connection = await _databaseConnectionFactory.CreateOpenConnection();
                var sqlScript = File.ReadAllText("./SqlScripts/CreateTables.sql");
                NpgsqlCommand command = new NpgsqlCommand(sqlScript, connection as NpgsqlConnection);
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to migrate database", ex);
                throw new Exception("Failed to migrate database", ex);
            }
        }

        /// <summary>
        /// Called when the application is stopping. No cleanup is required for this service.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for stopping the operation.</param>
        /// <returns>A completed task.</returns>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
