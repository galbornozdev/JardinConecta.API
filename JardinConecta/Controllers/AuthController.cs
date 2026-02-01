using JardinConecta.Common;
using JardinConecta.Http.Requests;
using JardinConecta.Http.Responses;
using JardinConecta.Infrastructure.Repository;
using JardinConecta.Models.Entities;
using JardinConecta.Services;
using Microsoft.AspNetCore.Authorization;
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

        [HttpGet("Me")]
        [Authorize]
        [ProducesResponseType(typeof(UsuarioLogueadoResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> Me()
        {
            var IdUsuarioLogueado = Guid.Parse(User.FindFirst(Constants.CUSTOM_CLAIMS__ID_USUARIO)?.Value!);

            var usuario = await _context.Set<Usuario>().AsNoTracking()
                .Include(x => x.Persona)
                .Include(x => x.UsuariosSalasRoles)
                    .ThenInclude(x => x.Sala)
                    .ThenInclude(x => x.Jardin)
                .Where(x => x.Id == IdUsuarioLogueado)
                .FirstAsync();

            var response = new UsuarioLogueadoResponse(
                    usuario.Email,
                    usuario.Persona!.Nombre,
                    usuario.Persona.Apellido,
                    usuario.Persona.Documento,
                    usuario.Persona.PhotoUrl,
                    usuario.UsuariosSalasRoles
                        .Select(x => new UsuarioLogueadoResponse_Jardin(
                            x.Sala.Jardin.Id,
                            x.Sala.Jardin.Nombre
                         ))
                        .DistinctBy(x => x.Id)
                        .ToList(),
                    usuario.UsuariosSalasRoles
                        .Select(x => new UsuarioLogueadoResponse_Sala(
                            x.Sala.Id,
                            x.Sala.Jardin.Id,
                            x.Sala.Nombre,
                            x.IdRol == (int)RolId.Educador
                            ))
                        .DistinctBy(x => x.Id)
                        .OrderByDescending(x => x.EsEducador)
                        .ToList());

            return Ok(response);
        }

        [HttpPost("Login")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

            string token;
            DateTime expires;

            if(usuario.TipoUsuario.Id == (int)TipoUsuarioId.AdminJardin)
                (token, expires) = _jwt.GenerateToken(usuario.Id, usuario.Email, usuario.TipoUsuario.Id.ToString(), usuario.IdJardin);
            else
                (token, expires) = _jwt.GenerateToken(usuario.Id, usuario.Email, usuario.TipoUsuario.Id.ToString());

            return Ok(new LoginResponse() { Token = token, Expires = expires });
        }
    }
}
