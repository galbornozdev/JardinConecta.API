using JardinConecta.Infrastructure.Repository;
using JardinConecta.Models.Entities;
using JardinConecta.Models.Http.Requests;
using JardinConecta.Models.Http.Responses;
using JardinConecta.Models.ViewModels.EmailTemplates;
using JardinConecta.Services.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JardinConecta.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class SalasController : AbstractController
    {
        private readonly IEmailService _emailService;

        public SalasController(ServiceContext context, IEmailService emailService) : base(context)
        {
            _emailService = emailService;
        }

        [HttpPost]
        [Authorize(Roles = $"{TipoUsuario.ROL_ADMIN_JARDIN},{TipoUsuario.ROL_ADMIN_SISTEMA}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(AltaSalaRequest request)
        {
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

            var sala = new Sala()
            {
                Id = Guid.NewGuid(),
                IdJardin = idJardin,
                Nombre = request.Nombre
            };

            await _context.AddAsync(sala);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(SalasResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAsync([FromQuery] Guid? idJardin)
        {
            int tipoUsuario = User.GetTipoUsuario();
            Guid? jardinFilter = tipoUsuario == (int)TipoUsuarioId.AdminJardin
                ? User.GetIdJardin()
                : idJardin;

            var result = await _context.Set<Sala>()
                .Where(x => jardinFilter == null || x.IdJardin == jardinFilter)
                .Select(x => new SalasResponse(x.Id, x.Nombre))
                .ToListAsync();

            return Ok(result);
        }

        [HttpGet("{salaId}/Miembros")]
        [Authorize(Roles = $"{TipoUsuario.ROL_ADMIN_JARDIN},{TipoUsuario.ROL_ADMIN_SISTEMA}")]
        [ProducesResponseType(typeof(ICollection<SalaMiembroResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMiembros(Guid salaId)
        {
            var miembros = await _context.Set<UsuarioSalaRol>()
                .Include(x => x.Usuario)
                    .ThenInclude(x => x.Persona)
                .Include(x => x.Usuario)
                    .ThenInclude(x => x.Tutelas)
                        .ThenInclude(t => t.Infante)
                            .ThenInclude(i => i.Salas)
                .Include(x => x.Usuario)
                    .ThenInclude(x => x.Tutelas)
                        .ThenInclude(t => t.TipoTutela)
                .Include(x => x.Rol)
                .Where(x => x.IdSala == salaId)
                .Select(x => new SalaMiembroResponse(
                    x.IdUsuario,
                    x.Usuario.Persona!.Nombre,
                    x.Usuario.Persona.Apellido,
                    x.Rol.Descripcion,
                    x.Usuario.Tutelas
                        .Where(t => t.Infante.Salas.Any(s => s.IdSala == salaId))
                        .Select(t => new TutelaInfo(
                            t.IdInfante,
                            t.Infante.Nombre,
                            t.Infante.Apellido,
                            t.TipoTutela.Descripcion))
                        .ToList()))
                .ToListAsync();

            return Ok(miembros);
        }

        [HttpPost("{salaId}/Educadores")]
        [Authorize(Roles = $"{TipoUsuario.ROL_ADMIN_JARDIN},{TipoUsuario.ROL_ADMIN_SISTEMA}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> AgregarEducador(Guid salaId, AgregarEducadorRequest request)
        {
            var sala = await _context.Set<Sala>()
                .Include(s => s.Jardin)
                .FirstOrDefaultAsync(s => s.Id == salaId);

            if (sala is null) return NotFound();

            var usuario = await _context.Set<Usuario>()
                .Include(u => u.Persona)
                .FirstOrDefaultAsync(u => u.Email == request.Email && u.IdJardin == sala.IdJardin && u.DeletedAt == null);

            if (usuario is null) return NotFound();

            var yaMiembro = await _context.Set<UsuarioSalaRol>()
                .AnyAsync(u => u.IdUsuario == usuario.Id && u.IdSala == salaId);

            if (yaMiembro) return Conflict();

            await _context.AddAsync(new UsuarioSalaRol
            {
                IdUsuario = usuario.Id,
                IdSala = salaId,
                IdRol = (int)RolId.Educador,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            await _emailService.SendTemplateAsync(usuario.Email, new AsignacionSalaViewModel
            {
                NombreEducador = usuario.Persona?.Nombre ?? usuario.Email,
                NombreSala = sala.Nombre,
                NombreJardin = sala.Jardin.Nombre
            });

            return NoContent();
        }

        [HttpDelete("{salaId}/Miembros/{usuarioId}")]
        [Authorize(Roles = $"{TipoUsuario.ROL_ADMIN_JARDIN},{TipoUsuario.ROL_ADMIN_SISTEMA}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteMiembro(Guid salaId, Guid usuarioId)
        {
            var miembro = await _context.Set<UsuarioSalaRol>()
                .FirstOrDefaultAsync(x => x.IdSala == salaId && x.IdUsuario == usuarioId);

            if (miembro == null) return NotFound();

            _context.Remove(miembro);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("Desvincular/{salaId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Desvincular(Guid salaId)
        {
            var idUsuario = User.GetIdUsuario();

            var miembro = await _context.Set<UsuarioSalaRol>()
                .FirstOrDefaultAsync(x => x.IdSala == salaId && x.IdUsuario == idUsuario);

            if (miembro == null) return NotFound();

            _context.Remove(miembro);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(SalaDetalleResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByIdAsync(Guid id)
        {
            var result = await _context.Set<Sala>()
                .Include(x => x.UsuariosSalasRoles)
                    .ThenInclude(x => x.Usuario)
                    .ThenInclude(x => x.Persona)
                .Where(x => x.Id == id)
                .Select(x => new SalaDetalleResponse(
                    x.Id,
                    x.Nombre,
                    x.UsuariosSalasRoles.Select(x => new SalaDetalleResponse_UsuariosMiembros(x.IdUsuario, x.Usuario.Persona!.Nombre, x.Usuario.Persona.Apellido)).ToList()))
                .FirstOrDefaultAsync();

            if (result == null)
                return NotFound();

            return Ok(result);
        }


    }
}
