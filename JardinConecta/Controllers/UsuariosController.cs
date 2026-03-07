using JardinConecta.Configurations;
using JardinConecta.Infrastructure.Repository;
using JardinConecta.Models.Entities;
using JardinConecta.Models.Http.Requests;
using JardinConecta.Models.ViewModels;
using JardinConecta.Models.ViewModels.EmailTemplates;
using JardinConecta.Services;
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

        public UsuariosController(
            ServiceContext context, IEmailService emailService,
            IOptions<ApplicationOptions> applicationOptions
        ) : base(context)
        {
            _emailService = emailService;
            _applicationOptions = applicationOptions.Value;
        }

        [HttpPost("RegistrarDispositivo")]
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
                    UpdatedAt = now
                };

                tokenVerificacionEmail = new TokenVerificacionEmail()
                {
                    Id = Guid.NewGuid(),
                    IdUsuario = usuario.Id,
                    Token = GenerateRandomString(),
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
                    Token = GenerateRandomString(),
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

        private static string GenerateRandomString(int length = 20)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var bytes = RandomNumberGenerator.GetBytes(length);

            return new string(bytes.Select(b => chars[b % chars.Length]).ToArray());
        }
    }
}
