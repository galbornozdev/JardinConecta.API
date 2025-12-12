using JardinConecta.Http.Requests;
using JardinConecta.Models.Entities;
using JardinConecta.Repository;
using JardinConecta.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JardinConecta.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly ServiceContext _context;

        public UsuariosController(ServiceContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Authorize(Roles = $"{TipoUsuario.ROL_ADMIN_JARDIN},{TipoUsuario.ROL_ADMIN_SISTEMA}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create(CreateUsuarioRequest request)
        {
            var now = DateTime.UtcNow;
            var idUsuarioLogueado = User.GetIdUsuario();
            var idRol = User.GetIdRol();

            Guid idJardin;
            if (idRol == (int)TipoUsuarioId.AdminJardin)
            {
                idJardin = User.GetIdJardin();
            }
            else
            {
                if (request.IdJardin == null) return BadRequest();
                idJardin = (Guid)request.IdJardin;

                var existeJardin = await _context.Set<Jardin>().Where(j => j.Id == request.IdJardin).AnyAsync();

                if (!existeJardin) return BadRequest();
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

            return Created();
        }
    }
}
