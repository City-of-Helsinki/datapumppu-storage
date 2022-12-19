using System.Data;
using Npgsql;

namespace Storage.Repositories.Providers
{
    public interface IDatabaseConnectionFactory
    {
        Task<NpgsqlConnection> CreateOpenConnection();
    }

    public class DatabaseConnectionFactory : IDatabaseConnectionFactory
    {
        private IConfiguration _configuration;

        public DatabaseConnectionFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<NpgsqlConnection> CreateOpenConnection()
        {
            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
            var connection = new NpgsqlConnection("Server=vpa-datapumppu.postgres.database.azure.com;Database=third-datapumppu;Port=5432;User Id=villepalo;Password=datapumppustorage17a76_userdev;Ssl Mode=VerifyFull;");
            await connection.OpenAsync();
            return connection;
        }
    }
}
