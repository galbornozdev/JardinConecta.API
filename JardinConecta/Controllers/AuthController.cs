using JardinConecta.Models.Entities;
using JardinConecta.Repository;
using JardinConecta.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace JardinConecta.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private IJwtService _jwt;
        private ServiceContext _context;

        public AuthController(IJwtService jwt, ServiceContext context)
        {
            _jwt = jwt;
            _context = context;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var usuario = await _context.Set<Usuario>()
                .AsNoTracking()
                .Include(u => u.TipoUsuario)
                .Where(u => u.Email.Equals(request.Email) && u.DeletedAt == null)
                .FirstOrDefaultAsync();

            if (usuario == null || !PasswordHasher.Verify(request.Password, usuario.PasswordHash))
            {
                return BadRequest();
            }

            (var token, var expires) = _jwt.GenerateToken(usuario.Id, usuario.Email, usuario.TipoUsuario.Id.ToString());

            return Ok(new { Token = token, Expires = expires });
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            var exists = await _context.Set<Usuario>()
                .AsNoTracking()
                .Where(u => u.Email.Equals(request.Email) && u.DeletedAt == null)
                .AnyAsync();

            if (exists)
            {
                return BadRequest();
            }

            var hashedPassword = PasswordHasher.Hash(request.Password);
            _context.Set<Usuario>().Add(new Usuario
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                PasswordHash = hashedPassword,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IdTipoUsuario = (int)TipoUsuarioId.Usuario,
                Telefono = new Telefono()
            });

            await _context.SaveChangesAsync();

            return Created();
        }
    }
}
