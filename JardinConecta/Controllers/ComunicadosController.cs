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
        [ProducesResponseType(typeof(Pagination<ComunicadoResponse>),StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPaginated([FromQuery] Guid idSala, [FromQuery] int page)
        {
            var total = await _context.Set<Comunicado>().Where(x => x.IdSala == idSala).CountAsync();
            var totalPages = (int)Math.Ceiling((decimal)total / Constants.DEFAULT_PAGE_SIZE);

            var items = await _context.Set<Comunicado>().Where(x => x.IdSala == idSala)
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * Constants.DEFAULT_PAGE_SIZE)
                .Take(Constants.DEFAULT_PAGE_SIZE)
                .ToListAsync();

            var pagination = new Pagination<ComunicadoResponse>(
                items.Select(x => new ComunicadoResponse(x.Id, x.Titulo, Limit(x.Contenido, 100), x.ComunicadoViews.Count)), 
                totalPages, 
                page, 
                Constants.DEFAULT_PAGE_SIZE);

            return Ok(pagination);
        }

        [HttpPost]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create(AltaComunicadoRequest request)
        {
            var idUsuario = User.GetIdUsuario();
            var idTipoUsuario = User.GetTipoUsuario();

            if (idTipoUsuario == (int)TipoUsuarioId.AdminJardin)
            {
                Guid idJardin = User.GetIdJardin();

                var check = await _context.Set<Sala>()
                    .Where(x => x.Id == request.SalaId && x.IdJardin == idJardin)
                    .AnyAsync();

                if (!check) return Forbid();
            }
            else
            {
                var check = await _context.Set<UsuarioSalaRol>()
                    .Where(x => x.IdSala == request.SalaId && x.IdUsuario == idUsuario && x.IdRol == (int)RolId.Educador)
                    .AnyAsync();

                if (!check) return Forbid();
            }

            var comunicado = new Comunicado()
            {
                Id = Guid.NewGuid(),
                IdSala = request.SalaId,
                IdUsuario = idUsuario,
                Titulo = request.Title,
                Contenido = request.Text
            };

            await _context.AddAsync(comunicado);

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
