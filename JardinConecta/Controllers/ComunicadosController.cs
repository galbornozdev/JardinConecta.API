using JardinConecta.Common;
using JardinConecta.Http.Requests;
using JardinConecta.Http.Responses;
using JardinConecta.Infrastructure.Repository;
using JardinConecta.Models.Entities;
using JardinConecta.Models.Mongo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace JardinConecta.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ComunicadosController : ControllerBase
    {
        private readonly ServiceContext _context;
        private IMongoCollection<Comunicado> _comunicadosCollection;
        private IMongoCollection<ComunicadoView> _comunicadosViewsCollection;

        public ComunicadosController(IMongoDatabase mongoDatabase, ServiceContext context)
        {
            _comunicadosCollection = mongoDatabase.GetCollection<Comunicado>(Comunicado.COLLECTION_NAME);
            _comunicadosViewsCollection = mongoDatabase.GetCollection<ComunicadoView>(ComunicadoView.COLLECTION_NAME);
            _context = context;
        }

        [HttpGet]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPaginated([FromQuery] Guid idSala, [FromQuery] int page)
        {
            var total = await _comunicadosCollection.CountDocumentsAsync(x => x.SalaId == idSala);
            var totalPages = (int)Math.Ceiling((decimal)total / Constants.DEFAULT_PAGE_SIZE);

            var items = await _comunicadosCollection.Find(x => x.SalaId == idSala)
                .SortByDescending(x => x.CreatedAt)
                .Skip((page - 1) * Constants.DEFAULT_PAGE_SIZE)
                .Limit(Constants.DEFAULT_PAGE_SIZE)
                .ToListAsync();

            var pagination = new Pagination<ComunicadoResponse>(items.Select(x => new ComunicadoResponse(x.Id, x.Title)), totalPages, page, Constants.DEFAULT_PAGE_SIZE);

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
                SalaId = request.SalaId,
                SenderId = idUsuario,
                Title = request.Title,
                Text = request.Text
            };

            await _comunicadosCollection.InsertOneAsync(comunicado);

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
                ComunicadoId = id,
                UserId = idUsuario
            };

            await _comunicadosViewsCollection.InsertOneAsync(view);

            return Ok();
        }

        [HttpGet("{id}/Views")]
        [ProducesResponseType(typeof(IEnumerable<Guid>),StatusCodes.Status200OK)]
        public async Task<IActionResult> GetViews(Guid id)
        {
            var items = await _comunicadosViewsCollection.Find(x => x.ComunicadoId == id).ToListAsync();

            return Ok(items.Select(x => x.UserId));
        }
    }
}
