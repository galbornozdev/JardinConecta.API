using JardinConecta.Services.Application.Dtos;

namespace JardinConecta.Services.Application.Interfaces
{
    public interface ISalasService
    {
        Task AsociarEducadorMedianteEmail(Guid salaId, string email);
        Task<bool> CheckSalaPerteneceJardin(Guid idJardin, Guid idSala);
        Task<bool> CheckUsuarioPerteneceASala(Guid idSala, Guid idUsuario, int? rol = null);
        Task CrearSala(Guid idJardin, string nombre);
        Task DesasociarUsuario(Guid idSala, Guid idUsuario);
        Task<List<SalaMiembroResult>> ObtenerMiembros(Guid salaId);
        Task<SalaDetalleResult> ObtenerSala(Guid idSala);
        Task<List<SalaResult>> ObtenerSalas(Guid? idJardin);
    }
}
