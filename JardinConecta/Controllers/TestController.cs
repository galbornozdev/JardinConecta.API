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

        [HttpGet("TestGuid")]
        public string TestGuid()
        {
            return Guid.NewGuid().ToString();
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

        [HttpPost("UploadFile")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest();

            //validate extentions
            IList<string> validExtentions = [".jpg" , ".png"];
            var fileExtension = Path.GetExtension(file.FileName);

            if (!validExtentions.Contains(fileExtension))
                return BadRequest();

            //validate file size
            //if(file.Length > (5 * 1024 * 1024)) //5MB
            //    return BadRequest();

            
            var fileName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + "_" + DateTime.UtcNow.ToString("yyyyMMddHHmmssfff") + fileExtension ;

            var uploadDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
            var filePath = Path.Combine(uploadDirectory, fileName);

            if (!Directory.Exists(uploadDirectory))
                Directory.CreateDirectory(uploadDirectory);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Ok();
        }
    }
}
