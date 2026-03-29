using JardinConecta.Models.Entities;
using JardinConecta.Services.Application.Dtos;

namespace JardinConecta.Services.Application.Interfaces
{
    public interface ICodigosDeInvitacionService
    {
        Task CanjearCodigo(Guid idUsuario, string codigo, string? documentoSufijo = null, int? idTipoTutela = null);
        Task<CodigoInvitacionResult> GenerarCodigoInvitacionSala(Guid idJardin, Guid idSala, DateTime fechaExpiracion, TipoInvitacion tipoInvitacion, Guid? idInfante = null);
        Task<List<CodigoInvitacionItemResult>> ListarCodigosInvitacion(Guid idJardin, Guid idSala);
        Task<VerificarInvitacionResult> VerificarCodigo(string codigo);
    }
}
