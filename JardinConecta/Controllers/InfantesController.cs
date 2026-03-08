using JardinConecta.Infrastructure.Repository;
using JardinConecta.Models.Entities;
using JardinConecta.Models.Http.Requests;
using JardinConecta.Models.Http.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JardinConecta.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class InfantesController : AbstractController
    {
        public InfantesController(ServiceContext context) : base(context)
        {
        }

        [HttpPost]
        [Authorize(Roles = $"{TipoUsuario.ROL_ADMIN_JARDIN},{TipoUsuario.ROL_ADMIN_SISTEMA}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(AltaInfanteRequest request)
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

            bool existeInfante = await _context.Set<Infante>()
                .Where(i => i.IdJardin == idJardin && i.Documento == request.Documento && i.DeletedAt == null)
                .AnyAsync();

            if (existeInfante) return Conflict();

            var infante = new Infante()
            {
                Id = Guid.NewGuid(),
                IdJardin = idJardin,
                Nombre = request.Nombre,
                Apellido = request.Apellido,
                Documento = request.Documento,
                FechaNacimiento = request.FechaNacimiento
            };

            await _context.AddAsync(infante);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet]
        [Authorize(Roles = $"{TipoUsuario.ROL_ADMIN_JARDIN},{TipoUsuario.ROL_ADMIN_SISTEMA}")]
        [ProducesResponseType(typeof(List<InfantesResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] Guid? idJardin, [FromQuery] Guid? idSala)
        {
            int tipoUsuario = User.GetTipoUsuario();
            Guid jardinId = tipoUsuario == (int)TipoUsuarioId.AdminJardin
                ? User.GetIdJardin()
                : idJardin ?? Guid.Empty;

            var query = _context.Set<Infante>()
                .Include(i => i.Salas).ThenInclude(s => s.Sala)
                .Where(i => i.IdJardin == jardinId && i.DeletedAt == null);

            if (idSala.HasValue)
                query = query.Where(i => i.Salas.Any(s => s.IdSala == idSala.Value));

            var result = await query
                .Select(i => new InfantesResponse(
                    i.Id,
                    i.Nombre,
                    i.Apellido,
                    i.Documento,
                    i.PhotoUrl,
                    i.FechaNacimiento,
                    i.Salas.Select(s => new InfantesResponse_Sala(s.IdSala, s.Sala.Nombre)).ToList()
                ))
                .ToListAsync();

            return Ok(result);
        }

        [HttpGet("{infanteId}")]
        [Authorize(Roles = $"{TipoUsuario.ROL_ADMIN_JARDIN},{TipoUsuario.ROL_ADMIN_SISTEMA}")]
        [ProducesResponseType(typeof(InfanteDetalleResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid infanteId)
        {
            var infante = await _context.Set<Infante>()
                .Include(i => i.Salas).ThenInclude(s => s.Sala)
                .Include(i => i.Tutelas).ThenInclude(t => t.TipoTutela)
                .Include(i => i.Tutelas).ThenInclude(t => t.Usuario).ThenInclude(u => u.Persona)
                .Where(i => i.Id == infanteId && i.DeletedAt == null)
                .Select(i => new InfanteDetalleResponse(
                    i.Id,
                    i.Nombre,
                    i.Apellido,
                    i.Documento,
                    i.PhotoUrl,
                    i.FechaNacimiento,
                    i.Tutelas.Select(t => new InfanteDetalleResponse_Tutela(
                        t.IdUsuario,
                        t.Usuario.Persona!.Nombre,
                        t.Usuario.Persona.Apellido,
                        t.TipoTutela.Descripcion
                    )).ToList(),
                    i.Salas.Select(s => new InfantesResponse_Sala(s.IdSala, s.Sala.Nombre)).ToList()
                ))
                .FirstOrDefaultAsync();

            if (infante == null) return NotFound();

            return Ok(infante);
        }

        [HttpPut("{infanteId}")]
        [Authorize(Roles = $"{TipoUsuario.ROL_ADMIN_JARDIN},{TipoUsuario.ROL_ADMIN_SISTEMA}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid infanteId, EditarInfanteRequest request)
        {
            var infante = await _context.Set<Infante>().FirstOrDefaultAsync(i => i.Id == infanteId && i.DeletedAt == null);

            if (infante == null) return NotFound();

            infante.Nombre = request.Nombre;
            infante.Apellido = request.Apellido;
            infante.Documento = request.Documento;
            infante.FechaNacimiento = request.FechaNacimiento;
            infante.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{infanteId}")]
        [Authorize(Roles = $"{TipoUsuario.ROL_ADMIN_JARDIN},{TipoUsuario.ROL_ADMIN_SISTEMA}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Delete(Guid infanteId)
        {
            var infante = await _context.Set<Infante>().FirstOrDefaultAsync(i => i.Id == infanteId && i.DeletedAt == null);

            if (infante == null) return NotFound();

            bool tieneSalas = await _context.Set<InfanteSala>().AnyAsync(x => x.IdInfante == infanteId);

            if (tieneSalas) return Conflict();

            infante.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{infanteId}/Salas/{salaId}")]
        [Authorize(Roles = $"{TipoUsuario.ROL_ADMIN_JARDIN},{TipoUsuario.ROL_ADMIN_SISTEMA}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DesasignarSala(Guid infanteId, Guid salaId)
        {
            var asignacion = await _context.Set<InfanteSala>()
                .FirstOrDefaultAsync(x => x.IdInfante == infanteId && x.IdSala == salaId);

            if (asignacion == null) return NotFound();

            _context.Remove(asignacion);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("{infanteId}/Salas/{salaId}")]
        [Authorize(Roles = $"{TipoUsuario.ROL_ADMIN_JARDIN},{TipoUsuario.ROL_ADMIN_SISTEMA}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> AsignarSala(Guid infanteId, Guid salaId)
        {
            var infante = await _context.Set<Infante>().FirstOrDefaultAsync(i => i.Id == infanteId && i.DeletedAt == null);
            if (infante == null) return NotFound();

            var sala = await _context.Set<Sala>().FindAsync(salaId);
            if (sala == null) return NotFound();

            bool yaAsignado = await _context.Set<InfanteSala>()
                .AnyAsync(x => x.IdInfante == infanteId && x.IdSala == salaId);

            if (yaAsignado) return Conflict();

            await _context.AddAsync(new InfanteSala { IdInfante = infanteId, IdSala = salaId });
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
