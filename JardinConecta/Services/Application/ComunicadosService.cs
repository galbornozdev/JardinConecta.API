using JardinConecta.Common;
using JardinConecta.Infrastructure.Repository;
using JardinConecta.Models.Entities;
using JardinConecta.Services.Application.Dtos;
using JardinConecta.Services.Application.Interfaces;
using JardinConecta.Services.Infrastructure;
using Microsoft.EntityFrameworkCore;
using static JardinConecta.Common.Helpers;

namespace JardinConecta.Services.Application
{
    public class ComunicadosService : IComunicadosService
    {
        private readonly ServiceContext _context;
        private readonly IFileStorageService _fileStorageService;
        private readonly ISalaNotificationService _salaNotificationService;

        private readonly int _archivosSizeLimitMB = 10;
        private readonly string[] _archivosAllowedTypes = { "image/jpeg", "image/png", "video/mp4", "video/quicktime" };

        public ComunicadosService(
            ServiceContext context,
            IFileStorageService fileStorageService,
            ISalaNotificationService salaNotificationService
        )
        {
            _context = context;
            _fileStorageService = fileStorageService;
            _salaNotificationService = salaNotificationService;
        }

        public async Task<PagedResult<ComunicadoItemResult>> ObtenerComunicadosPaginados(
            Guid idSala,
            int idTipoUsuario,
            Guid idUsuario,
            ComunicadosFilterDto filtros)
        {
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
            else if (filtros.Estado.HasValue)
                query = query.Where(x => x.Estado == filtros.Estado.Value);

            if (filtros.FechaDesde.HasValue)
                query = query.Where(x => x.FechaPublicacion >= filtros.FechaDesde.Value.ToUniversalTime());

            if (filtros.FechaHasta.HasValue)
                query = query.Where(x => x.FechaPublicacion < filtros.FechaHasta.Value.ToUniversalTime().AddDays(1));

            var total = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((decimal)total / Constants.DEFAULT_PAGE_SIZE);

            var items = await query
                .Include(c => c.Usuario)
                .ThenInclude(u => u.Persona)
                .Include(c => c.Views)
                .OrderByDescending(x => x.FechaPublicacion ?? x.FechaPrograma ?? x.UpdatedAt)
                .Skip((filtros.Page - 1) * Constants.DEFAULT_PAGE_SIZE)
                .Take(Constants.DEFAULT_PAGE_SIZE)
                .Select(x => new ComunicadoItemResult(
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

            return new PagedResult<ComunicadoItemResult>(items, totalPages, filtros.Page, Constants.DEFAULT_PAGE_SIZE);
        }

        public async Task<ComunicadoDetalleResult> ObtenerComunicado(Guid id, Guid idUsuario, int idTipoUsuario)
        {
            var comunicado = await _context.Set<Comunicado>()
                .Include(c => c.Usuario)
                .ThenInclude(u => u.Persona)
                .Include(c => c.Views)
                .Include(c => c.Archivos)
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();

            if (comunicado == null)
            {
                throw new KeyNotFoundException("Comunicado no encontrado");
            }

            var result = new ComunicadoDetalleResult(
                comunicado.Id,
                comunicado.Titulo,
                comunicado.Contenido,
                $"{comunicado.Usuario.Persona!.Nombre} {comunicado.Usuario.Persona.Apellido}",
                comunicado.Views.Count,
                comunicado.CreatedAt,
                comunicado.Archivos.Select(x => new ComunicadoArchivoResult(
                    x.Id,
                    _fileStorageService.BaseUrl + x.Id.ToString() + x.Extension,
                    x.NombreArchivoOriginal,
                    x.ContentType
                    )).ToList(),
                comunicado.Estado,
                comunicado.FechaPrograma,
                comunicado.UpdatedAt
                );

            if (idTipoUsuario == (int)TipoUsuarioId.Usuario)
            {
                await MarcarComunicadoComoVisto(comunicado, idUsuario, idTipoUsuario);
            }

            return result;
        }

        private async Task MarcarComunicadoComoVisto(Comunicado comunicado, Guid idUsuario, int idTipoUsuario)
        {
            var esFamiliaYNoVisto = await _context.Set<UsuarioSalaRol>()
                .Where(x => x.IdSala == comunicado.IdSala
                         && x.IdUsuario == idUsuario
                         && x.IdRol == (int)RolId.Familia)
                .AnyAsync(x => !_context.Set<ComunicadoView>()
                    .Any(v => v.IdComunicado == comunicado.Id && v.IdUsuario == idUsuario));

            if (esFamiliaYNoVisto)
            {
                var view = new ComunicadoView()
                {
                    IdComunicado = comunicado.Id,
                    IdUsuario = idUsuario
                };

                await _context.AddAsync(view);
                await _context.SaveChangesAsync();
            }
        }

        public async Task CrearNuevoComunicado(
            Guid idSala,
            Guid idUsuario,
            ComunicadoDto comunicadoData,
            List<IFormFile> archivos
        )
        {
            var comunicado = new Comunicado()
            {
                Id = Guid.NewGuid(),
                IdSala = idSala,
                IdUsuario = idUsuario,
                Titulo = comunicadoData.Titulo,
                Contenido = comunicadoData.Contenido,
                ContenidoTextoPlano = comunicadoData.ContenidoTextoPlano,
                Estado = comunicadoData.Estado,
                FechaPrograma = comunicadoData.Estado == (int)EstadoComunicado.Programado ? comunicadoData.FechaPrograma : null,
                FechaPublicacion = comunicadoData.Estado == (int)EstadoComunicado.Publicado ? DateTime.UtcNow : null
            };

            if (archivos != null && archivos.Any())
            {
                foreach (var file in archivos)
                {
                    if (file.Length > _archivosSizeLimitMB * 1024 * 1024)
                        throw new ArgumentException($"El archivo excede el limite de {_archivosSizeLimitMB}MB");

                    if (!_archivosAllowedTypes.Contains(file.ContentType))
                        throw new ArgumentException($"Solo se permiten archivos del tipo {string.Join(", ", _archivosAllowedTypes)}.");
                }

                foreach (var file in archivos)
                {
                    var idArchivo = Guid.NewGuid();
                    var safeFileName = $"{idArchivo}{Path.GetExtension(file.FileName)}";
                    await _fileStorageService.SaveAsync(file, safeFileName);

                    await _context.AddAsync(new ComunicadoArchivo()
                    {
                        Id = idArchivo,
                        IdComunicado = comunicado.Id,
                        NombreArchivoOriginal = file.FileName,
                        ContentType = file.ContentType,
                        Extension = Path.GetExtension(file.FileName),
                    });
                }
            }

            await _context.AddAsync(comunicado);
            await _context.SaveChangesAsync();

            if (comunicado.Estado == (int)EstadoComunicado.Publicado)
            {
                await _salaNotificationService.NotificarAsync(
                    comunicado.IdSala,
                    "Nuevo comunicado",
                    comunicado.Titulo,
                    excluirUsuario: idUsuario,
                    data: new Dictionary<string, string>
                    {
                        { "type", "comunicado" },
                        { "comunicadoId", comunicado.Id.ToString() }
                    });
            }
        }

        public async Task ModificarComunicado(
            Guid idComunicado,
            Guid idUsuario,
            ComunicadoDto comunicadoData,
            List<IFormFile>? archivos,
            List<Guid>? idsArchivosEliminar
        )
        {
            if (comunicadoData.Estado == (int)EstadoComunicado.Programado)
            {
                if (comunicadoData.FechaPrograma == null || comunicadoData.FechaPrograma <= DateTime.UtcNow)
                    throw new ArgumentException("FechaPrograma debe ser una fecha futura para comunicados programados");
            }

            var comunicado = await _context.Set<Comunicado>()
                .Include(c => c.Archivos)
                .Where(x => x.Id == idComunicado && x.IdUsuario == idUsuario)
                .FirstOrDefaultAsync();

            if (comunicado == null)
                throw new KeyNotFoundException("Comunicado no encontrado");

            if (comunicado.Estado == (int)EstadoComunicado.Publicado && comunicadoData.Estado == (int)EstadoComunicado.Programado)
                throw new InvalidOperationException("Un comunicado publicado no puede volver a estado programado");

            if (idsArchivosEliminar != null && idsArchivosEliminar.Any())
            {
                var archivosAEliminar = comunicado.Archivos
                    .Where(a => idsArchivosEliminar.Contains(a.Id))
                    .ToList();

                foreach (var archivo in archivosAEliminar)
                    _fileStorageService.Delete(archivo.Id.ToString() + archivo.Extension);

                _context.RemoveRange(archivosAEliminar);
            }

            if (archivos != null && archivos.Any())
            {
                foreach (var file in archivos)
                {
                    if (file.Length > _archivosSizeLimitMB * 1024 * 1024)
                        throw new ArgumentException($"El archivo excede el limite de {_archivosSizeLimitMB}MB");

                    if (!_archivosAllowedTypes.Contains(file.ContentType))
                        throw new ArgumentException($"Solo se permiten archivos del tipo {string.Join(", ", _archivosAllowedTypes)}.");
                }

                foreach (var file in archivos)
                {
                    var idArchivo = Guid.NewGuid();
                    var safeFileName = $"{idArchivo}{Path.GetExtension(file.FileName)}";
                    await _fileStorageService.SaveAsync(file, safeFileName);

                    await _context.AddAsync(new ComunicadoArchivo()
                    {
                        Id = idArchivo,
                        IdComunicado = comunicado.Id,
                        NombreArchivoOriginal = file.FileName,
                        ContentType = file.ContentType,
                        Extension = Path.GetExtension(file.FileName),
                    });
                }
            }

            var eraPublicado = comunicado.Estado == (int)EstadoComunicado.Publicado;

            comunicado.Titulo = comunicadoData.Titulo;
            comunicado.Contenido = comunicadoData.Contenido;
            comunicado.ContenidoTextoPlano = comunicadoData.ContenidoTextoPlano;
            comunicado.FechaPrograma = comunicadoData.Estado == (int)EstadoComunicado.Programado ? comunicadoData.FechaPrograma : null;

            if (!eraPublicado && comunicadoData.Estado == (int)EstadoComunicado.Publicado)
                comunicado.FechaPublicacion = DateTime.UtcNow;
            else if (comunicadoData.Estado != (int)EstadoComunicado.Publicado)
                comunicado.FechaPublicacion = null;

            if (eraPublicado)
                comunicado.UpdatedAt = DateTime.UtcNow;

            comunicado.Estado = comunicadoData.Estado;

            await _context.SaveChangesAsync();
        }

        public async Task PublicarComunicado(Guid id, Guid idUsuario)
        {
            var comunicado = await _context.Set<Comunicado>()
                .Where(x => x.Id == id && x.IdUsuario == idUsuario)
                .FirstOrDefaultAsync();

            if (comunicado == null)
                throw new KeyNotFoundException("Comunicado no encontrado");

            if (comunicado.Estado != (int)EstadoComunicado.Borrador)
                throw new ArgumentException("Solo se pueden publicar comunicados en estado Borrador");

            comunicado.Estado = (int)EstadoComunicado.Publicado;
            comunicado.FechaPublicacion = DateTime.UtcNow;
            comunicado.FechaPrograma = null;
            comunicado.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _salaNotificationService.NotificarAsync(
                comunicado.IdSala,
                "Nuevo comunicado",
                comunicado.Titulo,
                excluirUsuario: idUsuario,
                data: new Dictionary<string, string>
                {
                    { "type", "comunicado" },
                    { "comunicadoId", comunicado.Id.ToString() }
                });
        }

        public async Task EliminarComunicado(Guid id, Guid idUsuario)
        {
            var comunicado = await _context.Set<Comunicado>()
                .Include(c => c.Archivos)
                .Where(x => x.Id == id && x.IdUsuario == idUsuario)
                .FirstOrDefaultAsync();

            if (comunicado == null)
                throw new KeyNotFoundException("Comunicado no encontrado");

            if (comunicado.Estado != (int)EstadoComunicado.Borrador)
                throw new InvalidOperationException("Solo se pueden eliminar comunicados en estado Borrador");

            if (comunicado.Archivos.Any())
            {
                foreach (var archivo in comunicado.Archivos)
                    _fileStorageService.Delete(archivo.Id.ToString() + archivo.Extension);

                _context.RemoveRange(comunicado.Archivos);
            }

            _context.Remove(comunicado);
            await _context.SaveChangesAsync();
        }

        public async Task<List<ComunicadoViewDetalleResult>> ObtenerViews(Guid id)
        {
            var comunicado = await _context.Set<Comunicado>()
                .Where(x => x.Id == id)
                .Select(x => new { x.IdSala })
                .FirstOrDefaultAsync();

            if (comunicado == null)
                throw new KeyNotFoundException("Comunicado no encontrado");

            var views = await _context.Set<ComunicadoView>()
                .Where(v => v.IdComunicado == id)
                .Select(v => new
                {
                    NombreCompleto = v.Usuario.Persona!.Nombre + " " + v.Usuario.Persona.Apellido,
                    PhotoUrl = v.Usuario.Persona.PhotoUrl,
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

            var result = views.Select(v =>
            {
                string? photo = v.PhotoUrl;
                if (!string.IsNullOrEmpty(photo) && !photo.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    photo = _fileStorageService.BaseUrl + photo;
                }

                return new ComunicadoViewDetalleResult(
                    v.NombreCompleto,
                    v.ViewedAt,
                    photo,
                    v.Tutelas.Select(t => new TutelaDetalleResult(t.TipoTutela, t.NombreInfante)).ToList()
                );
            }).ToList();

            return result;
        }
    }
}
