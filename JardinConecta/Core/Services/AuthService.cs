using JardinConecta.Core.Common;
using JardinConecta.Core.Entities;
using JardinConecta.Core.Exceptions;
using JardinConecta.Core.Interfaces;
using JardinConecta.Core.Interfaces.Infrastructure;
using JardinConecta.Core.Services.Dtos;
using JardinConecta.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;

namespace JardinConecta.Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly ServiceContext _context;
        private readonly ITokenService _tokenService;

        public AuthService(
            ServiceContext context,
            ITokenService tokenService
        )
        {
            _context = context;
            _tokenService = tokenService;
        }

        public async Task<LoginResult> Login(string email, string password)
        {
            var usuario = await _context.Set<Usuario>()
                .AsNoTracking()
                .Include(u => u.TipoUsuario)
                .Where(u => u.Email.Equals(email) && u.DeletedAt == null && u.FechaVerificacionEmail != null)
                .FirstOrDefaultAsync();

            if (usuario == null || !PasswordHasher.Verify(password, usuario.PasswordHash))
            {
                throw new AuthenticationException("El email o la contraseña son incorrectos.");
            }

            string token;
            DateTime expires;

            if (usuario.TipoUsuario.Id == (int)TipoUsuarioId.AdminJardin)
                (token, expires) = _tokenService.GenerateToken(usuario.Id, usuario.Email, usuario.TipoUsuario.Id.ToString(), usuario.IdJardin);
            else
                (token, expires) = _tokenService.GenerateToken(usuario.Id, usuario.Email, usuario.TipoUsuario.Id.ToString());

            return new LoginResult(token, expires);
        }
    }
}
