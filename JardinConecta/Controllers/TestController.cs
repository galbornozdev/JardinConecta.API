using JardinConecta.Infrastructure.Repository;
using JardinConecta.Models.Entities;
using JardinConecta.Models.ViewModels.EmailTemplates;
using JardinConecta.Services;
using JardinConecta.Services.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace JardinConecta.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : AbstractController
    {
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;
        private readonly INotificationService _notificationService;

        public TestController(
            ServiceContext context,
            IEmailService emailService,
            ISmsService smsService,
            INotificationService notificationService
            ) : base(context)
        {
            _emailService = emailService;
            _smsService = smsService;
            _notificationService = notificationService;
        }

        public class TestPushRequest
        {
            public string DeviceToken { get; set; }
            public string Title { get; set; }
            public string Body { get; set; }
        }

        [HttpPost("TestPush")]
        public async Task<IActionResult> TestPush([FromBody] TestPushRequest request)
        {
            var result = await _notificationService.SendPushAsync(
                request.DeviceToken,
                request.Title,
                request.Body
            );

            if (!result.IsSuccess)
                return StatusCode(500, result.Error);

            return Ok();
        }

        public class MailToRequest
        {
            public string To { get; set; }
        }

        public class TestWelcomeMailRequest
        {
            public string To { get; set; }
            public string Name { get; set; }
        }

        public class TestMailRequest
        {
            public string To { get; set; }
            public string Subject { get; set; }
            public string Body { get; set; }
            public bool IsHtml { get; set; }
        }

        public class TestSMSRequest
        {
            public Guid IdUsuario { get; set; }
        }

        [HttpPost("TestSms")]
        public async Task<IActionResult> TestWelcomeMail(TestSMSRequest request)
        {
            try
            {
                var usuario = await _context.Set<Usuario>()
                    .Include(x => x.Persona)
                    .FirstAsync(x => x.Id == request.IdUsuario);

                var result = await _smsService.SendAsync(usuario!.Telefono.NumeroCompleto, $"Hola {usuario.Persona!.Nombre}, este es un mensaje de prueba desde el endpoint TestSms");
            }
            catch (Exception ex)
            {
                throw;
            }

            return Ok();
        }

        [HttpPost("TestWelcomeMail")]
        public async Task<IActionResult> TestWelcomeMail(TestWelcomeMailRequest request)
        {
            try
            {
                var result = await _emailService.SendTemplateAsync(request.To, new BienvenidaViewModel { Name = request.Name });
                if (!result.IsSuccess) return StatusCode(500, result.Error);
            }
            catch (Exception ex)
            {
                throw;
            }

            return Ok();
        }

        [HttpPost("TestMail")]
        public async Task<IActionResult> TestMail(TestMailRequest request)
        {
            var result = await _emailService.SendAsync(request.To, request.Subject, request.Body, request.IsHtml);

            if(!result.IsSuccess) return StatusCode(500, result.Error);

            return Ok();
        }

        [HttpGet("TestUUID")]
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
