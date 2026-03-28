using JardinConecta.Common;
using JardinConecta.Configurations;
using JardinConecta.Exceptions;
using JardinConecta.Infrastructure.Repository;
using JardinConecta.Models.Entities;
using JardinConecta.Models.ViewModels.EmailTemplates;
using JardinConecta.Services.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace JardinConecta.Services.Application
{
    public class OnboardingService : IOnboardingService
    {
        private readonly ServiceContext _context;
        private readonly IEmailService _emailService;
        private readonly ApplicationOptions _applicationOptions;

        public OnboardingService(
            ServiceContext context,
            IEmailService emailService,
            IOptions<ApplicationOptions> applicationOptions
        )
        {
            _context = context;
            _emailService = emailService;
            _applicationOptions = applicationOptions.Value;

        }

        public async Task AltaDeUsuario(string email, string password)
        {
            TokenVerificacionEmail tokenVerificacionEmail = null!;
            Result emailResult = null!;
            var now = DateTime.UtcNow;
            var fechaExpiracionToken = now.AddDays(1);

            Usuario? usuario = await _context.Set<Usuario>()
                .Where(x => x.Email == email)
                .FirstOrDefaultAsync();

            if (usuario == null)
            {
                usuario = new Usuario()
                {
                    Id = Guid.NewGuid(),
                    Email = email,
                    PasswordHash = PasswordHasher.Hash(password),
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
            else if (usuario.FechaVerificacionEmail == null)
            {
                usuario.UpdatedAt = now;
                usuario.PasswordHash = PasswordHasher.Hash(password);

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
                throw new InvalidOperationException("El email ya se encuentra registrado");
            }

            await _context.AddAsync(tokenVerificacionEmail);

            emailResult = await _emailService.SendTemplateAsync(
                usuario.Email,
                new VerificacionEmailViewModel() { BaseUrl = _applicationOptions.BaseUrl, Token = tokenVerificacionEmail.Token }
            );

            if (!emailResult.IsSuccess)
            {
                throw new ExternalServiceException("Error al enviar email de verificación");
            }

            await _context.SaveChangesAsync();
        }
    }
}
