using JardinConecta.Core.Services.Dtos;

namespace JardinConecta.Core.Interfaces
{
    public interface IComunicadosService
    {
        Task CrearNuevoComunicado(Guid idSala, Guid idUsuario, ComunicadoDto nuevoComunicado, List<IFormFile> archivos);
        Task EliminarComunicado(Guid id, Guid idUsuario);
        Task ModificarComunicado(Guid idComunicado, Guid idUsuario, ComunicadoDto comunicadoData, List<IFormFile>? archivos, List<Guid>? idsArchivosEliminar);
        Task<ComunicadoDetalleResult> ObtenerComunicado(Guid id, Guid idUsuario, int idTipoUsuario);
        Task<PagedResult<ComunicadoItemResult>> ObtenerComunicadosPaginados(Guid? idSala, int idTipoUsuario, Guid idUsuario, Guid? idJardin, ComunicadosFilterDto filtros);
        Task<List<ComunicadoViewDetalleResult>> ObtenerViews(Guid id);
        Task PublicarComunicado(Guid id, Guid idUsuario);
    }
}
