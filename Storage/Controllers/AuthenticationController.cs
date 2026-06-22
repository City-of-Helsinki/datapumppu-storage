using Microsoft.AspNetCore.Mvc;
using Storage.Repositories;
using System.Security.Cryptography;

namespace Storage.Controllers
{
    /// <summary>
    /// Provides API endpoints for user authentication and authorization.
    /// </summary>
    [ApiController]
    [Route("api/auth")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IAdminUsersRepository _adminUsersRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationController"/> class.
        /// </summary>
        /// <param name="configuration">Application configuration for accessing password salt.</param>
        /// <param name="adminUsersRepository">Repository for admin user data access.</param>
        public AuthenticationController(
            IConfiguration configuration,
            IAdminUsersRepository adminUsersRepository)
        {
            _configuration = configuration;
            _adminUsersRepository = adminUsersRepository;
        }

        /// <summary>
        /// Validates user credentials by checking username and hashed password against the database.
        /// </summary>
        /// <param name="username">The username to validate.</param>
        /// <param name="password">The password to validate (will be hashed with salt before comparison).</param>
        /// <returns>Returns 200 OK if user exists with matching credentials, 404 Not Found otherwise.</returns>
        [HttpGet("validate")]
        public async Task<IActionResult> IsValidUser(
            [FromQuery] string username,
            [FromQuery] string password)
        {
            using var hasher = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.ASCII.GetBytes(_configuration["PASSWORD_SALT"] + password);
            var passwordHash = Convert.ToBase64String(hasher.ComputeHash(bytes));

            var exists = await _adminUsersRepository.UserExists(new Repositories.Models.AdminUser
            {
                Username = username,
                Password = passwordHash
            });

            return exists ? Ok() : NotFound();
        }
    }
}
