using JardinConecta.Models.Entities;
using JardinConecta.Models.Http.Responses;
using JardinConecta.Services.Application.Dtos;

namespace JardinConecta.Services.Application.Interfaces
{
    public interface IComunicadosService
    {
        Task CrearNuevoComunicado(Guid idSala, Guid idUsuario, ComunicadoDto nuevoComunicado, List<IFormFile> archivos);
        Task EliminarComunicado(Guid id, Guid idUsuario);
        Task ModificarComunicado(Guid idComunicado, Guid idUsuario, ComunicadoDto comunicadoData, List<IFormFile>? archivos, List<Guid>? idsArchivosEliminar);
        Task<ComunicadoResponse> ObtenerComunicado(Guid id, Guid idUsuario, int idTipoUsuario);
        Task<Pagination<ComunicadoItemResponse>> ObtenerComunicadosPaginados(Guid idSala, int idTipoUsuario, Guid idUsuario, ComunicadosFilterDto filtros);
        Task<List<ComunicadoViewDetalleResponse>> ObtenerViews(Guid id);
        Task PublicarComunicado(Guid id, Guid idUsuario);
    }
}