using JardinConecta.Common;
using JardinConecta.Http.Requests;
using JardinConecta.Http.Responses;
using JardinConecta.Infrastructure.Repository;
using JardinConecta.Models.Entities;
using JardinConecta.Services;
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
        private readonly IFileStorageService _fileStorageService;

        public ComunicadosController(
            ServiceContext context,
            IFileStorageService fileStorageService
        )
        {
            _context = context;
            _fileStorageService = fileStorageService;
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
                .Include(c => c.Views)
                .Where(x => x.IdSala == idSala)
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * Constants.DEFAULT_PAGE_SIZE)
                .Take(Constants.DEFAULT_PAGE_SIZE)
                .Select(x => new ComunicadoItemResponse(
                    x.Id,
                    x.Titulo,
                    Limit(x.ContenidoTextoPlano, 100),
                    $"{x.Usuario.Persona!.Nombre} {x.Usuario.Persona.Apellido}",
                    x.Views.Count,
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
            var comunicado = await _context.Set<Comunicado>()
                .Include(c => c.Usuario)
                .ThenInclude(u => u.Persona)
                .Include(c => c.Views)
                .Include(c => c.Archivos)
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();

            if(comunicado == null)
            {
                return BadRequest(new { message = "Comunicado no encontrado" });
            }

            var result = new ComunicadoResponse(
                comunicado.Id,
                comunicado.Titulo,
                comunicado.Contenido,
                $"{comunicado.Usuario.Persona!.Nombre} {comunicado.Usuario.Persona.Apellido}",
                comunicado.Views.Count,
                comunicado.CreatedAt,
                comunicado.Archivos.Select(x => new ComunicadoArchivoResponse(
                    _fileStorageService.BaseUrl + x.Id.ToString() + x.Extension,
                    x.NombreArchivoOriginal,
                    x.ContentType
                    )).ToList()
                );

            var idUsuario = User.GetIdUsuario();
            var idTipoUsuario = User.GetTipoUsuario();

            if (idTipoUsuario == (int)TipoUsuarioId.Usuario)
            {
                var esFamiliaYNoVisto = await _context.Set<UsuarioSalaRol>()
                    .Where(x => x.IdSala == comunicado.IdSala
                             && x.IdUsuario == idUsuario
                             && x.IdRol == (int)RolId.Familia)
                    .AnyAsync(x => !_context.Set<ComunicadoView>()
                        .Any(v => v.IdComunicado == id && v.IdUsuario == idUsuario));

                if (esFamiliaYNoVisto)
                {
                    var view = new ComunicadoView()
                    {
                        IdComunicado = id,
                        IdUsuario = idUsuario
                    };

                    await _context.AddAsync(view);
                    await _context.SaveChangesAsync();
                }
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

            var comunicado = new Comunicado()
            {
                Id = Guid.NewGuid(),
                IdSala = request.IdSala,
                IdUsuario = idUsuario,
                Titulo = request.Titulo,
                Contenido = request.Contenido,
                ContenidoTextoPlano = request.ContenidoTextoPlano
            };

            if (request.Archivos != null && request.Archivos.Any())
            {
                foreach (var file in request.Archivos)
                {
                    if (file.Length > 10 * 1024 * 1024) // 10MB limit
                    {
                        return BadRequest(new { message = "File size exceeds 10MB limit" });
                    }

                    var allowedTypes = new[] { "image/jpeg", "image/png", "video/mp4", "video/quicktime" };

                    if (!allowedTypes.Contains(file.ContentType))
                    {
                        return BadRequest(new { message = "Only JPEG and PNG files are allowed" });
                    }

                }

                foreach (var file in request.Archivos)
                {
                    var idArchivo = Guid.NewGuid();
                    var safeFileName = $"{idArchivo}{Path.GetExtension(file.FileName)}";
                    var fileName = await _fileStorageService.SaveAsync(file, safeFileName);

                    var comunicadoArchivo = new ComunicadoArchivo()
                    {
                        Id = idArchivo,
                        IdComunicado = comunicado.Id,
                        NombreArchivoOriginal = file.FileName,
                        ContentType = file.ContentType,
                        Extension = Path.GetExtension(file.FileName),
                    };

                    await _context.AddAsync(comunicadoArchivo);
                }
            }

            await _context.AddAsync(comunicado);
            await _context.SaveChangesAsync();

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
