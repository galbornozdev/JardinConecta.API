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
        private IMongoCollection<Comunicado> _collection;

        public ComunicadosController(IMongoDatabase mongoDatabase, ServiceContext context)
        {
            _collection = mongoDatabase.GetCollection<Comunicado>(Comunicado.COLLECTION_NAME);
            _context = context;
        }

        [HttpGet]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPaginated([FromQuery] Guid idSala, [FromQuery] int page)
        {
            var total = await _collection.CountDocumentsAsync(x => x.SalaId == idSala.ToString());
            var totalPages = (int)Math.Ceiling((decimal)total / Constants.DEFAULT_PAGE_SIZE);

            var items = await _collection.Find(x => x.SalaId == idSala.ToString())
                .SortByDescending(x => x.CreatedAt)
                .Skip((page - 1) * Constants.DEFAULT_PAGE_SIZE)
                .Limit(Constants.DEFAULT_PAGE_SIZE)
                .ToListAsync();

            var pagination = new Pagination<Comunicado>(items, totalPages, page, Constants.DEFAULT_PAGE_SIZE);

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
                SalaId = request.SalaId.ToString(),
                SenderId = idUsuario.ToString(),
                Title = request.Title,
                Text = request.Text
            };

            await _collection.InsertOneAsync(comunicado);

            return Ok();
        }
    }
}
