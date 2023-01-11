using Microsoft.AspNetCore.Mvc;
using Storage.Repositories;
using System.Security.Cryptography;

namespace Storage.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IAdminUsersRepository _adminUsersRepository;

        public AuthenticationController(
            IConfiguration configuration,
            IAdminUsersRepository adminUsersRepository)
        {
            _configuration = configuration;
            _adminUsersRepository = adminUsersRepository;
        }

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
