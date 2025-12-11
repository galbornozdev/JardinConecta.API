using JardinConecta.Http.Requests;
using JardinConecta.Models.Entities;
using JardinConecta.Repository;
using JardinConecta.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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

        //[HttpGet]
        //public async Task<IEnumerable<Usuario>> Get()
        //{
        //    var usuarios = await _context.Set<Usuario>().ToListAsync();
        //    return usuarios;
        //}

        [HttpPost]
        [Authorize(Roles = $"{TipoUsuario.ROL_ADMIN_JARDIN},{TipoUsuario.ROL_ADMIN_SISTEMA}")]
        public async Task<IActionResult> Create(CreateUsuarioRequest request)
        {
            var now = DateTime.UtcNow;

            var IdUsuarioLogueado = Guid.Parse(User.FindFirst("uid")?.Value!);
            var IdRol = User.FindFirstValue(ClaimTypes.Role);

            Guid? IdJardin = null;
            if (IdRol == TipoUsuario.ROL_ADMIN_JARDIN)
            {
                IdJardin = await _context.Set<Usuario>()
                    .AsNoTracking()
                    .Where(u => u.Id == IdUsuarioLogueado).Select(u => u.IdJardin).FirstOrDefaultAsync();
            }
            else
            {
                IdJardin = request.IdJardin;

                if (IdJardin == null) return BadRequest();

                var existeJardin = await _context.Set<Jardin>().Where(j => j.Id == request.IdJardin).AnyAsync();

                if (!existeJardin) return BadRequest();
            }

            bool existeUsuario = await _context.Set<Usuario>().Where(u => u.IdJardin == IdJardin && u.Email == request.Email).AnyAsync();

            if (existeUsuario) return Forbid();

            if(request.Salas.Count > 0)
            {
                var countSalas = await _context.Set<Sala>().Where(s => s.IdJardin == IdJardin && request.Salas.Contains(s.Id)).CountAsync();
                if (countSalas < request.Salas.Count) return Forbid();
            }
            var rol = request.EsEducador ? (int)RolId.Educador : (int)RolId.Tutor;
            var idUsuario = Guid.NewGuid();
            var usuario = new Usuario()
            {
                Id = idUsuario,
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



            await _context.Set<Usuario>().AddAsync(usuario);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
