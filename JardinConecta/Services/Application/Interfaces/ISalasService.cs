using JardinConecta.Models.Http.Responses;

namespace JardinConecta.Services.Application.Interfaces
{
    public interface ISalasService
    {
        Task AsociarEducadorMedianteEmail(Guid salaId, string email);
        Task<bool> CheckSalaPerteneceJardin(Guid idJardin, Guid idSala);
        Task<bool> CheckUsuarioPerteneceASala(Guid idSala, Guid idUsuario, int? rol = null);
        Task CrearSala(Guid idJardin, string nombre);
        Task DesasociarUsuario(Guid idSala, Guid idUsuario);
        Task<List<SalaMiembroResponse>> ObtenerMiembros(Guid salaId);
        Task<SalaDetalleResponse> ObtenerSala(Guid idSala);
        Task<List<SalasResponse>> ObtenerSalas(Guid? idJardin);
    }
}