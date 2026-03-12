using JardinConecta.Common;
using JardinConecta.Infrastructure.Repository;
using JardinConecta.Models.Entities;
using JardinConecta.Models.Http.Requests;
using JardinConecta.Models.Http.Responses;
using JardinConecta.Services.Application;
using JardinConecta.Services.Infrastructure;
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
        private readonly ISalaNotificationService _salaNotificationService;

        public ComunicadosController(
            ServiceContext context,
            IFileStorageService fileStorageService,
            ISalaNotificationService salaNotificationService
        )
        {
            _context = context;
            _fileStorageService = fileStorageService;
            _salaNotificationService = salaNotificationService;
        }

        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(Pagination<ComunicadoItemResponse>),StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPaginated(
            [FromQuery] Guid idSala,
            [FromQuery] int page,
            [FromQuery] int? estado,
            [FromQuery] DateTime? fechaDesde,
            [FromQuery] DateTime? fechaHasta)
        {
            var idTipoUsuario = User.GetTipoUsuario();
            var idUsuario = User.GetIdUsuario();

            bool puedeVerNoPublicados;
            if (idTipoUsuario == (int)TipoUsuarioId.AdminJardin || idTipoUsuario == (int)TipoUsuarioId.AdminSistema)
            {
                puedeVerNoPublicados = true;
            }
            else
            {
                puedeVerNoPublicados = await _context.Set<UsuarioSalaRol>()
                    .AnyAsync(x => x.IdSala == idSala && x.IdUsuario == idUsuario && x.IdRol == (int)RolId.Educador);
            }

            var query = _context.Set<Comunicado>().Where(x => x.IdSala == idSala);

            if (!puedeVerNoPublicados)
                query = query.Where(x => x.Estado == (int)EstadoComunicado.Publicado);
            else if (estado.HasValue)
                query = query.Where(x => x.Estado == estado.Value);

            if (fechaDesde.HasValue)
                query = query.Where(x => x.FechaPublicacion >= fechaDesde.Value.ToUniversalTime());

            if (fechaHasta.HasValue)
                query = query.Where(x => x.FechaPublicacion < fechaHasta.Value.ToUniversalTime().AddDays(1));

            var total = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((decimal)total / Constants.DEFAULT_PAGE_SIZE);

            var items = await query
                .Include(c => c.Usuario)
                .ThenInclude(u => u.Persona)
                .Include(c => c.Views)
                .OrderByDescending(x => x.FechaPublicacion ?? x.FechaPrograma ?? x.UpdatedAt)
                .Skip((page - 1) * Constants.DEFAULT_PAGE_SIZE)
                .Take(Constants.DEFAULT_PAGE_SIZE)
                .Select(x => new ComunicadoItemResponse(
                    x.Id,
                    x.Titulo,
                    Limit(x.ContenidoTextoPlano, 100),
                    $"{x.Usuario.Persona!.Nombre} {x.Usuario.Persona.Apellido}",
                    x.Views.Count,
                    x.CreatedAt,
                    x.Estado,
                    x.FechaPublicacion,
                    x.FechaPrograma))
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
                    x.Id,
                    _fileStorageService.BaseUrl + x.Id.ToString() + x.Extension,
                    x.NombreArchivoOriginal,
                    x.ContentType
                    )).ToList(),
                comunicado.Estado,
                comunicado.FechaPrograma,
                comunicado.UpdatedAt
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

            if (request.Estado == (int)EstadoComunicado.Programado)
            {
                if (request.FechaPrograma == null || request.FechaPrograma <= DateTime.UtcNow)
                    return BadRequest(new { message = "FechaPrograma debe ser una fecha futura para comunicados programados" });
            }

            var comunicado = new Comunicado()
            {
                Id = Guid.NewGuid(),
                IdSala = request.IdSala,
                IdUsuario = idUsuario,
                Titulo = request.Titulo,
                Contenido = request.Contenido,
                ContenidoTextoPlano = request.ContenidoTextoPlano,
                Estado = request.Estado,
                FechaPrograma = request.Estado == (int)EstadoComunicado.Programado ? request.FechaPrograma : null,
                FechaPublicacion = request.Estado == (int)EstadoComunicado.Publicado ? DateTime.UtcNow : null
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

            if (comunicado.Estado == (int)EstadoComunicado.Publicado)
                await _salaNotificationService.NotificarAsync(comunicado.IdSala, "Nuevo comunicado", comunicado.Titulo, excluirUsuario: idUsuario);

            return Ok();
        }

        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromForm] EditarComunicadoRequest request)
        {
            var idUsuario = User.GetIdUsuario();

            var comunicado = await _context.Set<Comunicado>()
                .Include(c => c.Archivos)
                .Where(x => x.Id == id && x.IdUsuario == idUsuario)
                .FirstOrDefaultAsync();

            if (comunicado == null)
                return NotFound(new { message = "Comunicado no encontrado" });

            if (request.Estado == (int)EstadoComunicado.Programado)
            {
                if (request.FechaPrograma == null || request.FechaPrograma <= DateTime.UtcNow)
                    return BadRequest(new { message = "FechaPrograma debe ser una fecha futura para comunicados programados" });
            }

            if (comunicado.Estado == (int)EstadoComunicado.Publicado &&
                request.Estado == (int)EstadoComunicado.Programado)
                return BadRequest(new { message = "Un comunicado publicado no puede volver a estado programado" });

            if (request.ArchivosEliminar != null && request.ArchivosEliminar.Any())
            {
                var archivosAEliminar = comunicado.Archivos
                    .Where(a => request.ArchivosEliminar.Contains(a.Id))
                    .ToList();

                foreach (var archivo in archivosAEliminar)
                    _fileStorageService.Delete(archivo.Id.ToString() + archivo.Extension);

                _context.RemoveRange(archivosAEliminar);
            }

            if (request.Archivos != null && request.Archivos.Any())
            {
                foreach (var file in request.Archivos)
                {
                    if (file.Length > 10 * 1024 * 1024)
                        return BadRequest(new { message = "File size exceeds 10MB limit" });

                    var allowedTypes = new[] { "image/jpeg", "image/png", "video/mp4", "video/quicktime" };
                    if (!allowedTypes.Contains(file.ContentType))
                        return BadRequest(new { message = "Only JPEG and PNG files are allowed" });
                }

                foreach (var file in request.Archivos)
                {
                    var idArchivo = Guid.NewGuid();
                    var safeFileName = $"{idArchivo}{Path.GetExtension(file.FileName)}";
                    await _fileStorageService.SaveAsync(file, safeFileName);

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

            var eraPublicado = comunicado.Estado == (int)EstadoComunicado.Publicado;

            comunicado.Titulo = request.Titulo;
            comunicado.Contenido = request.Contenido;
            comunicado.ContenidoTextoPlano = request.ContenidoTextoPlano;
            comunicado.FechaPrograma = request.Estado == (int)EstadoComunicado.Programado ? request.FechaPrograma : null;

            if (!eraPublicado && request.Estado == (int)EstadoComunicado.Publicado)
                comunicado.FechaPublicacion = DateTime.UtcNow;
            else if (request.Estado != (int)EstadoComunicado.Publicado)
                comunicado.FechaPublicacion = null;

            if (eraPublicado)
                comunicado.UpdatedAt = DateTime.UtcNow;

            comunicado.Estado = request.Estado;

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("{id}/Publicar")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Publicar(Guid id)
        {
            var idUsuario = User.GetIdUsuario();

            var comunicado = await _context.Set<Comunicado>()
                .Where(x => x.Id == id && x.IdUsuario == idUsuario)
                .FirstOrDefaultAsync();

            if (comunicado == null)
                return NotFound(new { message = "Comunicado no encontrado" });

            if (comunicado.Estado != (int)EstadoComunicado.Borrador)
                return BadRequest(new { message = "Solo se pueden publicar comunicados en estado Borrador" });

            comunicado.Estado = (int)EstadoComunicado.Publicado;
            comunicado.FechaPublicacion = DateTime.UtcNow;
            comunicado.FechaPrograma = null;
            comunicado.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _salaNotificationService.NotificarAsync(comunicado.IdSala, "Nuevo comunicado", comunicado.Titulo, excluirUsuario: idUsuario);

            return Ok();
        }

        [HttpGet("{id}/Views")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<ComunicadoViewDetalleResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetViews(Guid id)
        {
            var idUsuario = User.GetIdUsuario();
            var idTipoUsuario = User.GetTipoUsuario();

            var comunicado = await _context.Set<Comunicado>()
                .Where(x => x.Id == id)
                .Select(x => new { x.IdSala })
                .FirstOrDefaultAsync();

            if (comunicado == null)
                return NotFound(new { message = "Comunicado no encontrado" });

            if (idTipoUsuario != (int)TipoUsuarioId.AdminJardin &&
                idTipoUsuario != (int)TipoUsuarioId.AdminSistema)
            {
                var esEducador = await _context.Set<UsuarioSalaRol>()
                    .AnyAsync(x => x.IdSala == comunicado.IdSala
                                && x.IdUsuario == idUsuario
                                && x.IdRol == (int)RolId.Educador);

                if (!esEducador) return Forbid();
            }

            var views = await _context.Set<ComunicadoView>()
                .Where(v => v.IdComunicado == id)
                .Select(v => new
                {
                    NombreCompleto = v.Usuario.Persona!.Nombre + " " + v.Usuario.Persona.Apellido,
                    v.ViewedAt,
                    Tutelas = v.Usuario.Tutelas
                        .Where(t => t.Infante.Salas.Any(s => s.IdSala == comunicado.IdSala))
                        .Select(t => new
                        {
                            TipoTutela = t.TipoTutela.Descripcion,
                            NombreInfante = t.Infante.Nombre + " " + t.Infante.Apellido,
                        })
                        .ToList()
                })
                .ToListAsync();

            var result = views.SelectMany(v => v.Tutelas.Select(t => new ComunicadoViewDetalleResponse(
                v.NombreCompleto,
                t.TipoTutela,
                t.NombreInfante,
                v.ViewedAt
            )));

            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var idUsuario = User.GetIdUsuario();

            var comunicado = await _context.Set<Comunicado>()
                .Include(c => c.Archivos)
                .Where(x => x.Id == id && x.IdUsuario == idUsuario)
                .FirstOrDefaultAsync();

            if (comunicado == null)
                return NotFound(new { message = "Comunicado no encontrado" });

            if (comunicado.Estado != (int)EstadoComunicado.Borrador)
                return BadRequest(new { message = "Solo se pueden eliminar comunicados en estado Borrador" });

            if (comunicado.Archivos.Any())
            {
                foreach (var archivo in comunicado.Archivos)
                    _fileStorageService.Delete(archivo.Id.ToString() + archivo.Extension);

                _context.RemoveRange(comunicado.Archivos);
            }

            _context.Remove(comunicado);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
