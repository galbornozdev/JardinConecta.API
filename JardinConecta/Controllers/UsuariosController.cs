using JardinConecta.Http.Requests;
using JardinConecta.Infrastructure.Repository;
using JardinConecta.Models.Entities;
using JardinConecta.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JardinConecta.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsuariosController : AbstractController
    {
        public UsuariosController(ServiceContext context) : base(context)
        {
        }

        [HttpPost]
        [Authorize(Roles = $"{TipoUsuario.ROL_ADMIN_JARDIN},{TipoUsuario.ROL_ADMIN_SISTEMA}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create(AltaUsuarioRequest request)
        {
            var now = DateTime.UtcNow;
            var idUsuarioLogueado = User.GetIdUsuario();

            Guid idJardin;
            try
            {
                idJardin = await SelectIdJardin(request);
            }
            catch (InvalidOperationException)
            {
                return BadRequest();
            }

            bool existeUsuario = await _context.Set<Usuario>().Where(u => u.IdJardin == idJardin && u.Email == request.Email).AnyAsync();

            if (existeUsuario) return Forbid();

            if(request.Salas.Count > 0)
            {
                var countSalas = await _context.Set<Sala>().Where(s => s.IdJardin == idJardin && request.Salas.Contains(s.Id)).CountAsync();
                if (countSalas < request.Salas.Count) return Forbid();
            }
            var rol = request.EsEducador ? (int)RolId.Educador : (int)RolId.Tutor;
            var usuario = new Usuario()
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                PasswordHash = PasswordHasher.Hash(request.Password),
                Telefono = new Telefono()
                {
                    CaracteristicaPais = request.CaracteristicaPais,
                    CodigoArea = request.CodigoArea,
                    Numero = request.Numero,
                },
                CreatedAt = now,
                UpdatedAt = now,
                IdTipoUsuario = (int)TipoUsuarioId.Usuario,
                Tutelas = request.EsEducador ? [] : 
                    request.Tutelas.Select(t => 
                        new Tutela() { IdTipoTutela = t.IdTipo, IdInfante = t.IdInfante }
                        ).ToList(),

                Persona = new Persona() {
                    Documento = request.Documento,
                    Nombre = request.Nombre,
                    Apellido = request.Apellido
                },

                UsuariosSalasRoles = (request.Salas.Count > 0) ? 
                    request.Salas.Select(x => new UsuarioSalaRol { IdRol = rol, IdSala = x }).ToList() : []
            };

            await _context.AddAsync(usuario);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
