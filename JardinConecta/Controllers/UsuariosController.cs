using JardinConecta.Common;
using JardinConecta.Configurations;
using JardinConecta.Infrastructure.Repository;
using JardinConecta.Models.Entities;
using JardinConecta.Models.Http.Requests;
using JardinConecta.Models.ViewModels;
using JardinConecta.Models.ViewModels.EmailTemplates;
using JardinConecta.Services;
using JardinConecta.Services.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace JardinConecta.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsuariosController : AbstractController
    {
        private readonly IEmailService _emailService;
        private readonly ApplicationOptions _applicationOptions;
        private readonly IWebHostEnvironment _env;

        public UsuariosController(
            ServiceContext context, IEmailService emailService,
            IOptions<ApplicationOptions> applicationOptions,
            IWebHostEnvironment env
        ) : base(context)
        {
            _emailService = emailService;
            _applicationOptions = applicationOptions.Value;
            _env = env;
        }

        [HttpPatch("Me")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ActualizarPerfil([FromBody] ActualizarPerfilRequest request)
        {
            var idUsuario = User.GetIdUsuario();

            var usuario = await _context.Set<Usuario>()
                .Include(x => x.Persona)
                .Where(x => x.Id == idUsuario)
                .FirstOrDefaultAsync();

            if (usuario == null)
            {
                return NotFound(new { Message = "Usuario no encontrado" });
            }

            if (usuario.Persona == null)
            {
                usuario.Persona = new Persona
                {
                    IdUsuario = usuario.Id,
                    Nombre = request.Nombre,
                    Apellido = request.Apellido,
                    Documento = request.Documento,
                };
                await _context.AddAsync(usuario.Persona);
            }
            else
            {
                usuario.Persona.Nombre = request.Nombre;
                usuario.Persona.Apellido = request.Apellido;
                usuario.Persona.Documento = request.Documento;
            }

            usuario.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("Me/Photo")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> SubirFoto(IFormFile photo)
        {
            if (photo == null || photo.Length == 0)
                return BadRequest();

            var ext = Path.GetExtension(photo.FileName).ToLowerInvariant();
            if (ext != ".jpg" && ext != ".jpeg" && ext != ".png")
                return BadRequest();

            if (photo.Length > 3 * 1024 * 1024) // 3 MB
                return BadRequest();

            var idUsuario = User.GetIdUsuario();

            var usuario = await _context.Set<Usuario>()
                .Include(x => x.Persona)
                .Where(x => x.Id == idUsuario)
                .FirstOrDefaultAsync();

            if (usuario == null) return NotFound();

            var profilesPath = Path.Combine(_env.ContentRootPath, "media", "profiles");
            Directory.CreateDirectory(profilesPath);

            var fileName = $"{idUsuario}_{DateTime.UtcNow:yyyyMMddHHmmssfff}{ext}";
            var filePath = Path.Combine(profilesPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await photo.CopyToAsync(stream);
            }

            var photoUrl = $"/media/profiles/{fileName}";

            if (usuario.Persona == null)
            {
                usuario.Persona = new Persona
                {
                    IdUsuario = usuario.Id,
                    Nombre = string.Empty,
                    Apellido = string.Empty,
                    PhotoUrl = photoUrl,
                };
                await _context.AddAsync(usuario.Persona);
            }
            else
            {
                usuario.Persona.PhotoUrl = photoUrl;
            }

            usuario.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { photoUrl });
        }

        [HttpPost("RegistrarDispositivo")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RegistrarDispositivo([FromBody] RegistrarDispositivoRequest request)
        {
            var idUsuario = User.GetIdUsuario();

            var usuario = await _context.Set<Usuario>()
                .Where(x => x.Id == idUsuario)
                .FirstOrDefaultAsync();

            if (usuario == null)
            {
                return NotFound(new { Message = "Usuario no encontrado" });
            }

            usuario.DeviceToken = request.DeviceToken;
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Dispositivo registrado correctamente" });
        }

        [HttpPost("")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Create([FromBody]AltaUsuarioRequest request)
        {
            TokenVerificacionEmail tokenVerificacionEmail = null!;
            Result emailResult = null!;
            var now = DateTime.UtcNow;
            var fechaExpiracionToken = now.AddHours(1);

            Usuario? usuario = await _context.Set<Usuario>()
                .Where(x => x.Email == request.Email)
                .FirstOrDefaultAsync();

            if (usuario == null)
            {
                usuario = new Usuario()
                {
                    Id = Guid.NewGuid(),
                    Email = request.Email,
                    PasswordHash = PasswordHasher.Hash(request.Password),
                    IdTipoUsuario = (int)TipoUsuarioId.Usuario,
                    CreatedAt = now,
                    UpdatedAt = now,
                    Telefono = new Telefono()
                };

                tokenVerificacionEmail = new TokenVerificacionEmail()
                {
                    Id = Guid.NewGuid(),
                    IdUsuario = usuario.Id,
                    Token = Helpers.GenerateRandomString(),
                    FechaExpiracion = fechaExpiracionToken,
                };

                await _context.AddAsync(usuario);
            }
            else if(usuario.FechaVerificacionEmail == null)
            {
                usuario.UpdatedAt = now;
                usuario.PasswordHash = PasswordHasher.Hash(request.Password);

                tokenVerificacionEmail = new TokenVerificacionEmail()
                {
                    Id = Guid.NewGuid(),
                    IdUsuario = usuario.Id,
                    Token = Helpers.GenerateRandomString(),
                    FechaExpiracion = fechaExpiracionToken
                };
            }
            else
            {
                return Forbid();
            }

            await _context.AddAsync(tokenVerificacionEmail);

            emailResult = await _emailService.SendTemplateAsync(
                usuario.Email,
                new VerificacionEmailViewModel() {BaseUrl = _applicationOptions.BaseUrl, Token = tokenVerificacionEmail.Token }
            );

            if (!emailResult.IsSuccess)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Error al enviar email de verificación" });
            }

            await _context.SaveChangesAsync();

            return Ok();
        }

    }
}
