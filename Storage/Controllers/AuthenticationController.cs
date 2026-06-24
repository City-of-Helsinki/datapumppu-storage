using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Storage.Repositories;
using System.Security.Cryptography;
using System.Text;

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
        /// Authenticates an admin user using credentials in the request body.
        /// </summary>
        /// <param name="login">Login data containing username and password.</param>
        /// <returns>Returns 200 OK on success, 401 Unauthorized on failure.</returns>
        [HttpPost("login")]
        [EnableRateLimiting("login")]
        public async Task<IActionResult> Login([FromBody] DTOs.LoginDTO login)
        {
            var user = await _adminUsersRepository.GetUserByUsername(login.Username);
            
            using var hasher = SHA256.Create();
            var pepper = _configuration["PASSWORD_SALT"] ?? string.Empty;
            var bytes = Encoding.ASCII.GetBytes(pepper + login.Password);
            var passwordHash = Convert.ToBase64String(hasher.ComputeHash(bytes));

            if (user == null || user.Password != passwordHash)
            {
                return Unauthorized();
            }

            return Ok();
        }
    }
}
