using JardinConecta.Core.Entities;
using JardinConecta.Core.Services.Dtos;

namespace JardinConecta.Core.Interfaces
{
    public interface ICodigosDeInvitacionService
    {
        Task CanjearCodigo(Guid idUsuario, string codigo, string? documentoSufijo = null, int? idTipoTutela = null);
        Task<CodigoInvitacionResult> GenerarCodigoInvitacionSala(Guid idJardin, Guid idSala, DateTime fechaExpiracion, TipoInvitacion tipoInvitacion, Guid? idInfante = null);
        Task<List<CodigoInvitacionItemResult>> ListarCodigosInvitacion(Guid idJardin, Guid idSala);
        Task<VerificarInvitacionResult> VerificarCodigo(string codigo);
    }
}
