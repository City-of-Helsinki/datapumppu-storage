using Microsoft.AspNetCore.Mvc;
using Storage.Repositories;
using System.Security.Cryptography;

namespace Storage.Controllers
{
    [ApiController]
    [Route("api/auth")]

    public class AuthenticationController
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
            using var md5 = System.Security.Cryptography.MD5.Create();
            var bytes = System.Text.Encoding.ASCII.GetBytes(_configuration["PASSWORD_SALT"] + password);
            var passwordHash = Convert.ToBase64String(md5.ComputeHash(bytes));

            var exists = await _adminUsersRepository.UserExists(new Repositories.Models.AdminUser
            {
                Username = username,
                Password = passwordHash
            });

            return exists ? new OkResult() : new ForbidResult();
        }
    }
}
