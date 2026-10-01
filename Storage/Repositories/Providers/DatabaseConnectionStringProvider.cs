using Npgsql;

namespace Storage.Repositories.Providers
{
    /// <summary>
    /// Builds the PostgreSQL connection string, allowing DATABASE_PASSWORD to override
    /// any password already present in STORAGE_DB_CONNECTION_STRING / Database:ConnectionString.
    /// </summary>
    public static class DatabaseConnectionStringProvider
    {
        public static string? GetConnectionString(IConfiguration configuration)
        {
            var connectionString = configuration["STORAGE_DB_CONNECTION_STRING"] ?? configuration["Database:ConnectionString"];
            var password = configuration["DATABASE_PASSWORD"];

            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(connectionString))
            {
                return connectionString;
            }

            var builder = new NpgsqlConnectionStringBuilder(connectionString)
            {
                Password = password
            };
            return builder.ConnectionString;
        }
    }
}
