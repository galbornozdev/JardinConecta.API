using JardinConecta.Common;
using JardinConecta.Configurations;
using JardinConecta.Exceptions;
using JardinConecta.Infrastructure.Repository;
using JardinConecta.Models.Entities;
using JardinConecta.Models.ViewModels.EmailTemplates;
using JardinConecta.Services.Application.Dtos;
using JardinConecta.Services.Application.Interfaces;
using JardinConecta.Services.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace JardinConecta.Services.Application
{
    public class UsuariosService : IUsuariosService
    {
        private readonly ServiceContext _context;
        private readonly IEmailService _emailService;
        private readonly IFileStorageService _fileStorageService;
        private readonly ApplicationOptions _applicationOptions;

        public UsuariosService(
            ServiceContext context,
            IEmailService emailService,
            IFileStorageService fileStorageService,
            IOptions<ApplicationOptions> applicationOptions
        )
        {
            _context = context;
            _emailService = emailService;
            _fileStorageService = fileStorageService;
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

        public async Task ActualizarInformacionPersonal(Guid idUsuario, string nombre, string apellido, string? documento = null)
        {
            var usuario = await _context.Set<Usuario>()
                .Include(x => x.Persona)
                .Where(x => x.Id == idUsuario)
                .FirstOrDefaultAsync();

            if (usuario == null)
            {
                throw new KeyNotFoundException("Usuario no encontrado");
            }

            if (usuario.Persona == null)
            {
                usuario.Persona = new Persona
                {
                    IdUsuario = usuario.Id,
                    Nombre = nombre,
                    Apellido = apellido,
                    Documento = documento,
                };
                await _context.AddAsync(usuario.Persona);
            }
            else
            {
                usuario.Persona.Nombre = nombre;
                usuario.Persona.Apellido = apellido;
                usuario.Persona.Documento = documento;
            }

            usuario.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task<string> ActualizarFotoPerfil(Guid idUsuario, IFormFile fotoPerfil)
        {
            var ext = Path.GetExtension(fotoPerfil.FileName).ToLowerInvariant();
            if (ext != ".jpg" && ext != ".jpeg" && ext != ".png")
                throw new ArgumentException("No se permite la extensión del archivo.");

            var limiteMB = 3;
            if (fotoPerfil.Length > limiteMB * 1024 * 1024)
                throw new ArgumentException($"No se permiten archivos superiores a {limiteMB}MB.");

            var usuario = await _context.Set<Usuario>()
                .Include(x => x.Persona)
                .Where(x => x.Id == idUsuario)
                .FirstOrDefaultAsync();

            if (usuario == null) throw new KeyNotFoundException("No se encontró el usuario.");

            var fileName = $"profile_{idUsuario}_{DateTime.UtcNow:yyyyMMddHHmmssfff}{ext}";

            await _fileStorageService.SaveAsync(fotoPerfil, fileName);

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

            return _fileStorageService.BaseUrl + fileName;
        }

        public async Task ActualizarDeviceToken(Guid idUsuario, string deviceToken)
        {
            var usuario = await _context.Set<Usuario>()
                .Where(x => x.Id == idUsuario)
                .FirstOrDefaultAsync();

            if (usuario == null)
            {
                throw new KeyNotFoundException("Usuario no encontrado");
            }

            usuario.DeviceToken = deviceToken;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> VerificarEmail(string token)
        {
            var now = DateTime.UtcNow;

            var tokenVerificacionEmail = await _context.Set<TokenVerificacionEmail>()
                .Include(x => x.Usuario)
                .Where(t => t.Token == token && t.FechaUtilizacion == null && t.FechaExpiracion > now)
                .FirstOrDefaultAsync();

            if (tokenVerificacionEmail == null)
            {
                return false;
            }

            tokenVerificacionEmail.FechaUtilizacion = now;
            tokenVerificacionEmail.Usuario.FechaVerificacionEmail = now;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<UsuarioLogueadoResult> ObtenerUsuario(Guid idUsuario)
        {
            var usuario = await _context.Set<Usuario>().AsNoTracking()
                .Include(x => x.Persona)
                .Include(x => x.UsuariosSalasRoles)
                    .ThenInclude(x => x.Sala)
                    .ThenInclude(x => x.Jardin)
                .Where(x => x.Id == idUsuario)
                .FirstAsync();

            string? photo = usuario.Persona?.PhotoUrl;
            if (!string.IsNullOrEmpty(photo) && !photo.StartsWith("http", System.StringComparison.OrdinalIgnoreCase))
            {
                photo = _fileStorageService.BaseUrl + photo;
            }

            return new UsuarioLogueadoResult(
                usuario.Id,
                usuario.Email,
                usuario.Persona?.Nombre,
                usuario.Persona?.Apellido,
                usuario.Persona?.Documento,
                photo,
                usuario.UsuariosSalasRoles
                    .Select(x => new UsuarioJardinResult(x.Sala.Jardin.Id, x.Sala.Jardin.Nombre))
                    .DistinctBy(x => x.Id)
                    .ToList(),
                usuario.UsuariosSalasRoles
                    .Select(x => new UsuarioSalaResult(
                        x.Sala.Id,
                        x.Sala.Jardin.Id,
                        x.Sala.Nombre,
                        x.IdRol == (int)RolId.Educador))
                    .DistinctBy(x => x.Id)
                    .OrderByDescending(x => x.EsEducador)
                    .ToList());
        }
    }
}
