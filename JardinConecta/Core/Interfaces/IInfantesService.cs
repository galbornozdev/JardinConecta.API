using JardinConecta.Core.Services.Dtos;
using Microsoft.AspNetCore.Http;

namespace JardinConecta.Core.Interfaces
{
    public interface IInfantesService
    {
        Task ActualizarInfante(Guid idInfante, string nombre, string apellido, int documento, DateTime fechaNacimiento);
        Task AltaDeInfante(Guid idJardin, string nombre, string apellido, int documento, DateTime fechaNacimiento);
        Task AsignarSala(Guid idInfante, Guid idSala);
        Task DesasignarSala(Guid idInfante, Guid idSala);
        Task DesasignarTutela(Guid infanteId, Guid usuarioId);
        Task EliminarInfante(Guid idInfante);
        Task<ImportarInfantesResult> ImportarInfantes(Guid idJardin, IFormFile csvFile);
        Task<InfanteDetalleResult> ObtenerInfante(Guid infanteId);
        Task<List<InfanteResult>> ObtenerInfantes(Guid idJardin, Guid? idSala);
    }
}
