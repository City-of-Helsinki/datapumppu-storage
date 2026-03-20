using System.Data;
using Npgsql;

namespace Storage.Repositories.Providers
{
    /// <summary>
    /// Factory interface for creating and opening database connections.
    /// </summary>
    public interface IDatabaseConnectionFactory
    {
        /// <summary>
        /// Creates and opens a new database connection.
        /// </summary>
        /// <returns>An open database connection ready for use.</returns>
        Task<IDbConnection> CreateOpenConnection();
    }

    /// <summary>
    /// Factory implementation for creating PostgreSQL database connections.
    /// Configures Dapper to match database column names with underscores to C# property names.
    /// </summary>
    public class DatabaseConnectionFactory : IDatabaseConnectionFactory
    {
        private IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseConnectionFactory"/> class.
        /// </summary>
        /// <param name="configuration">Application configuration containing the connection string.</param>
        public DatabaseConnectionFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Creates and opens a new PostgreSQL database connection.
        /// Configures Dapper to automatically map database column names with underscores (snake_case) 
        /// to C# property names (PascalCase).
        /// </summary>
        /// <returns>An open PostgreSQL connection ready for executing queries.</returns>
        /// <remarks>
        /// The connection string is read from the STORAGE_DB_CONNECTION_STRING configuration value.
        /// Callers are responsible for disposing the connection after use.
        /// </remarks>
        public async Task<IDbConnection> CreateOpenConnection()
        {
            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
            var connection = new NpgsqlConnection(_configuration["STORAGE_DB_CONNECTION_STRING"]);
            await connection.OpenAsync();
            return connection;
        }
    }
}
