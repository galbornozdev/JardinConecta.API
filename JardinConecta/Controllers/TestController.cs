using JardinConecta.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JardinConecta.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        public TestController()
        {
            
        }

        [HttpPost("TestHash")]
        public string TestHash(string str)
        {
            return PasswordHasher.Hash(str);
        }

        [Authorize]
        [HttpPost("TestSecurity")]
        public IActionResult TestSecurity()
        {
            return Ok();
        }

        [Authorize]
        [HttpPost("ReadClaims")]
        public IActionResult ReadClaims()
        {
            var userId = User.FindFirst("uid")?.Value;
            var email = User.FindFirstValue(ClaimTypes.Email);
            var role = User.FindFirstValue(ClaimTypes.Role);

            return Ok(new { userId, email, role });
        }

        [Authorize(Roles = "AdminSistema")]
        [HttpPost("TestSecurityRoles")]
        public IActionResult TestSecurityRoles()
        {
            return Ok();
        }
    }
}
