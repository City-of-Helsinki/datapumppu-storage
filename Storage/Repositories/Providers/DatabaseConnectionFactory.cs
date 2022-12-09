using System.Data;
using Npgsql;

namespace Storage.Repositories.Providers
{
    public interface IDatabaseConnectionFactory
    {
        Task<IDbConnection> CreateOpenConnection();
    }

    public class DatabaseConnectionFactory : IDatabaseConnectionFactory
    {
        private IConfiguration _configuration;

        public DatabaseConnectionFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IDbConnection> CreateOpenConnection()
        {
            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
            var connection = new NpgsqlConnection(_configuration["Database:ConnectionString"]);
            await connection.OpenAsync();
            return connection;
        }
    }
}
