using JardinConecta.Common;
using JardinConecta.Infrastructure.Repository;
using JardinConecta.Models.Entities;
using JardinConecta.Models.Http.Requests;
using JardinConecta.Models.Http.Responses;
using JardinConecta.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JardinConecta.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AdminController : AbstractController
    {
        public AdminController(ServiceContext context) : base(context)
        {
        }

        [HttpPost("Usuario")]
        [Authorize(Roles = $"{TipoUsuario.ROL_ADMIN_JARDIN},{TipoUsuario.ROL_ADMIN_SISTEMA}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create(AltaUsuarioPorAdminRequest request)
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
            var rol = request.EsEducador ? (int)RolId.Educador : (int)RolId.Familia;
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

        [HttpPost("Invitaciones")]
        [Authorize(Roles = $"{TipoUsuario.ROL_ADMIN_JARDIN},{TipoUsuario.ROL_ADMIN_SISTEMA}")]
        [ProducesResponseType(typeof(CodigoInvitacionResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GenerarInvitacion(AltaCodigoInvitacionRequest request)
        {
            Guid idJardin;
            try
            {
                idJardin = await SelectIdJardin(request);
            }
            catch (InvalidOperationException)
            {
                return BadRequest();
            }

            var sala = await _context.Set<Sala>().Where(s => s.Id == request.IdSala && s.IdJardin == idJardin).FirstOrDefaultAsync();
            if (sala is null) return BadRequest();

            var infante = await _context.Set<Infante>().Where(i => i.Id == request.IdInfante && i.IdJardin == idJardin && i.DeletedAt == null).FirstOrDefaultAsync();
            if (infante is null) return BadRequest();

            var perteneceASala = await _context.Set<InfanteSala>().AnyAsync(x => x.IdInfante == request.IdInfante && x.IdSala == request.IdSala);
            //if (!perteneceASala) return BadRequest();
            if (!perteneceASala)
                await _context.AddAsync(new InfanteSala { IdInfante = request.IdInfante, IdSala = request.IdSala });

            string codigo;
            bool colision;
            do
            {
                codigo = Helpers.GenerateRandomStringUpperCase();
                colision = await _context.Set<CodigoInvitacion>().AnyAsync(c => c.Codigo == codigo);
            } while (colision);

            var invitacion = new CodigoInvitacion
            {
                Id = Guid.NewGuid(),
                Codigo = codigo,
                IdSala = request.IdSala,
                IdInfante = request.IdInfante,
                FechaExpiracion = request.FechaExpiracion,
                CreatedAt = DateTime.UtcNow
            };

            await _context.AddAsync(invitacion);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GenerarInvitacion), new CodigoInvitacionResponse
            {
                Id = invitacion.Id,
                Codigo = invitacion.Codigo,
                IdSala = invitacion.IdSala,
                IdInfante = invitacion.IdInfante,
                FechaExpiracion = invitacion.FechaExpiracion
            });
        }

        [HttpGet("Invitaciones")]
        [Authorize(Roles = $"{TipoUsuario.ROL_ADMIN_JARDIN},{TipoUsuario.ROL_ADMIN_SISTEMA}")]
        [ProducesResponseType(typeof(List<CodigoInvitacionItemResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ListarInvitaciones([FromQuery] Guid idSala, [FromQuery] Guid? idJardin)
        {
            Guid jardinId;
            int tipoUsuario = User.GetTipoUsuario();
            if (tipoUsuario == (int)TipoUsuarioId.AdminJardin)
            {
                jardinId = User.GetIdJardin();
            }
            else
            {
                if (idJardin is null) return BadRequest();
                jardinId = idJardin.Value;
            }

            var salaExiste = await _context.Set<Sala>().AnyAsync(s => s.Id == idSala && s.IdJardin == jardinId);
            if (!salaExiste) return BadRequest();

            var now = DateTime.UtcNow;
            var invitaciones = await _context.Set<CodigoInvitacion>()
                .Where(c => c.IdSala == idSala)
                .Select(c => new CodigoInvitacionItemResponse
                {
                    Id = c.Id,
                    Codigo = c.Codigo,
                    NombreInfante = c.Infante.Nombre + " " + c.Infante.Apellido,
                    FechaExpiracion = c.FechaExpiracion,
                    EstaVigente = c.FechaExpiracion > now
                })
                .ToListAsync();

            return Ok(invitaciones);
        }
    }
}
