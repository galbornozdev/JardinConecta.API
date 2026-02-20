using JardinConecta.Common;
using JardinConecta.Http.Requests;
using JardinConecta.Http.Responses;
using JardinConecta.Infrastructure.Repository;
using JardinConecta.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static JardinConecta.Common.Helpers;

namespace JardinConecta.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ComunicadosController : ControllerBase
    {
        private readonly ServiceContext _context;

        public ComunicadosController(ServiceContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(Pagination<ComunicadoItemResponse>),StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPaginated([FromQuery] Guid idSala, [FromQuery] int page)
        {
            var total = await _context.Set<Comunicado>().Where(x => x.IdSala == idSala).CountAsync();
            var totalPages = (int)Math.Ceiling((decimal)total / Constants.DEFAULT_PAGE_SIZE);

            var items = await _context.Set<Comunicado>()
                .Include(c => c.Usuario)
                .ThenInclude(u => u.Persona)
                .Include(c => c.ComunicadoViews)
                .Where(x => x.IdSala == idSala)
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * Constants.DEFAULT_PAGE_SIZE)
                .Take(Constants.DEFAULT_PAGE_SIZE)
                .Select(x => new ComunicadoItemResponse(
                    x.Id,
                    x.Titulo,
                    Limit(x.ContenidoTextoPlano, 100),
                    $"{x.Usuario.Persona!.Nombre} {x.Usuario.Persona.Apellido}",
                    x.ComunicadoViews.Count,
                    x.CreatedAt))
                .ToListAsync();

            var pagination = new Pagination<ComunicadoItemResponse>(
                items,
                totalPages, 
                page, 
                Constants.DEFAULT_PAGE_SIZE);

            return Ok(pagination);
        }

        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(ComunicadoResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _context.Set<Comunicado>()
                .Include(c => c.Usuario)
                .ThenInclude(u => u.Persona)
                .Include(c => c.ComunicadoViews)
                .Where(x => x.Id == id)
                .Select(x => new ComunicadoResponse(
                    x.Id,
                    x.Titulo,
                    x.Contenido,
                    $"{x.Usuario.Persona!.Nombre} {x.Usuario.Persona.Apellido}",
                    x.ComunicadoViews.Count,
                    x.CreatedAt))
                .FirstOrDefaultAsync();

            if(result == null)
            {
                return BadRequest(new { message = "Comunicado no encontrado" });
            }

            return Ok(result);
        }

        [HttpPost]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create([FromForm] AltaComunicadoRequest request)
        {
            var idUsuario = User.GetIdUsuario();
            var idTipoUsuario = User.GetTipoUsuario();

            if (idTipoUsuario == (int)TipoUsuarioId.AdminJardin)
            {
                Guid idJardin = User.GetIdJardin();

                var check = await _context.Set<Sala>()
                    .Where(x => x.Id == request.IdSala && x.IdJardin == idJardin)
                    .AnyAsync();

                if (!check) return Forbid();
            }
            else
            {
                var check = await _context.Set<UsuarioSalaRol>()
                    .Where(x => x.IdSala == request.IdSala && x.IdUsuario == idUsuario && x.IdRol == (int)RolId.Educador)
                    .AnyAsync();

                if (!check) return Forbid();
            }

            // Validate files if any
            if (request.Archivos != null && request.Archivos.Any())
            {
                // Optional: Add file size/type validation
                foreach (var file in request.Archivos)
                {
                    if (file.Length > 10 * 1024 * 1024) // 10MB limit
                    {
                        return BadRequest(new { message = "File size exceeds 10MB limit" });
                    }
                }
            }

            var comunicado = new Comunicado()
            {
                Id = Guid.NewGuid(),
                IdSala = request.IdSala,
                IdUsuario = idUsuario,
                Titulo = request.Titulo,
                Contenido = request.Contenido,
                ContenidoTextoPlano = request.ContenidoTextoPlano
            };

            await _context.AddAsync(comunicado);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("{id}/Views")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> AddViewed(Guid id)
        {
            var idUsuario = User.GetIdUsuario();

            var view = new ComunicadoView()
            {
                IdComunicado = id,
                IdUsuario = idUsuario
            };

            await _context.AddAsync(view);

            return Ok();
        }

        [HttpGet("{id}/Views")]
        [ProducesResponseType(typeof(IEnumerable<Guid>),StatusCodes.Status200OK)]
        public async Task<IActionResult> GetViews(Guid id)
        {
            var items = await _context.Set<ComunicadoView>().Where(x => x.IdComunicado == id).ToListAsync();

            return Ok(items.Select(x => x.IdUsuario));
        }
    }
}
