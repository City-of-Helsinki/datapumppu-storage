using Dapper;
using Storage.Repositories.Models;
using Storage.Repositories.Providers;

namespace Storage.Repositories
{
    /// <summary>
    /// Provides data access methods for admin user authentication and validation.
    /// </summary>
    public interface IAdminUsersRepository
    {
        /// <summary>
        /// Checks if an admin user exists with the provided username and password.
        /// </summary>
        /// <param name="user">The admin user credentials to validate.</param>
        /// <returns>True if the user exists with matching credentials, otherwise false.</returns>
        Task<bool> UserExists(AdminUser user);
    }

    /// <summary>
    /// Implements admin user data access operations using Dapper for PostgreSQL queries.
    /// </summary>
    public class AdminUsersRepository : IAdminUsersRepository
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;

        /// <summary>
        /// Initializes a new instance of the AdminUsersRepository class.
        /// </summary>
        /// <param name="connectionFactory">Factory for creating database connections.</param>
        public AdminUsersRepository(IDatabaseConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        /// <summary>
        /// Checks if an admin user exists with the provided username and password.
        /// </summary>
        /// <param name="user">The admin user credentials to validate.</param>
        /// <returns>True if exactly one user exists with matching credentials, otherwise false.</returns>
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
