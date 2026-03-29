using JardinConecta.Models.Http.Responses;

namespace JardinConecta.Services.Application.Interfaces
{
    public interface IInfantesService
    {
        Task ActualizarInfante(Guid idInfante, string nombre, string apellido, int documento, DateTime fechaNacimiento);
        Task AltaDeInfante(Guid idJardin, string nombre, string apellido, int documento, DateTime fechaNacimiento);
        Task AsignarSala(Guid idInfante, Guid idSala);
        Task DesasignarSala(Guid idInfante, Guid idSala);
        Task DesasignarTutela(Guid infanteId, Guid usuarioId);
        Task EliminarInfante(Guid idInfante);
        Task<InfanteDetalleResponse> ObtenerInfante(Guid infanteId);
        Task<List<InfantesResponse>> ObtenerInfantes(Guid idJardin, Guid? idSala);
    }
}