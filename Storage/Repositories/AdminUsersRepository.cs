using Dapper;
using Storage.Repositories.Models;
using Storage.Repositories.Providers;

namespace Storage.Repositories
{
    public interface IAdminUsersRepository
    {
        Task<bool> UserExists(AdminUser user);
    }

    public class AdminUsersRepository : IAdminUsersRepository
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;

        public AdminUsersRepository(IDatabaseConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<bool> UserExists(AdminUser user)
        {
            var sql = @"
                select count(*) from admin_users where username = @username and password = @password
            ";

            using var connection = await _connectionFactory.CreateOpenConnection();
            var count = (await connection.QueryAsync<int>(sql, new { username = user.Username, password = user.Password })).First();

            return count == 1;
        }
    }
}
