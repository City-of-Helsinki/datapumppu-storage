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
        /// Retrieves an admin user's record from the database by their unique username.
        /// </summary>
        /// <param name="username">The username to search for.</param>
        /// <returns>The admin user's record if found, otherwise null.</returns>
        Task<AdminUser?> GetUserByUsername(string username);
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
        /// Retrieves an admin user's record from the database by their unique username.
        /// </summary>
        /// <param name="username">The username to search for.</param>
        /// <returns>The admin user's record if found, otherwise null.</returns>
        public async Task<AdminUser?> GetUserByUsername(string username)
        {
            var sql = @"
                select * from admin_users where username = @username
            ";

            using var connection = await _connectionFactory.CreateOpenConnection();
            return (await connection.QueryAsync<AdminUser>(sql, new { username })).FirstOrDefault();
        }
    }
}
