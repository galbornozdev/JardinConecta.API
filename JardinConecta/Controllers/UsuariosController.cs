using JardinConecta.Common;
using JardinConecta.Configurations;
using JardinConecta.Infrastructure.Repository;
using JardinConecta.Models.Entities;
using JardinConecta.Models.Http.Requests;
using JardinConecta.Models.ViewModels;
using JardinConecta.Models.ViewModels.EmailTemplates;
using JardinConecta.Services;
using JardinConecta.Services.Application;
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
        private readonly IFileStorageService _fileStorageService;
        private readonly IOnboardingService _onboardingService;

        public UsuariosController(
            ServiceContext context, IEmailService emailService,
            IOptions<ApplicationOptions> applicationOptions,
            IFileStorageService fileStorageService,
            IOnboardingService onboardingService
        ) : base(context)
        {
            _fileStorageService = fileStorageService;
            _onboardingService = onboardingService;
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

            var fileName = $"profile_{idUsuario}_{DateTime.UtcNow:yyyyMMddHHmmssfff}{ext}";

            await _fileStorageService.SaveAsync(photo, fileName);

            if (usuario.Persona == null)
            {
                usuario.Persona = new Persona
                {
                    IdUsuario = usuario.Id,
                    Nombre = string.Empty,
                    Apellido = string.Empty,
                    PhotoUrl = fileName,
                };
                await _context.AddAsync(usuario.Persona);
            }
            else
            {
                usuario.Persona.PhotoUrl = fileName;
            }

            usuario.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { photoUrl = _fileStorageService.BaseUrl + fileName });
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
            await _onboardingService.AltaDeUsuario(request.Email, request.Password);
            return Ok();
        }

    }
}
