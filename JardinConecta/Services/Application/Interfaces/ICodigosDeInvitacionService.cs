using JardinConecta.Models.Entities;
using JardinConecta.Models.Http.Responses;

namespace JardinConecta.Services.Application.Interfaces
{
    public interface ICodigosDeInvitacionService
    {
        Task CanjearCodigo(Guid idUsuario, string codigo, string? documentoSufijo = null, int? idTipoTutela = null);
        Task<CodigoInvitacionResponse> GenerarCodigoInvitacionSala(Guid idJardin, Guid idSala, DateTime fechaExpiracion, TipoInvitacion tipoInvitacion, Guid? idInfante = null);
        Task<List<CodigoInvitacionItemResponse>> ListarCodigosInvitacion(Guid idJardin, Guid idSala);
        Task<VerificarInvitacionResponse> VerificarCodigo(string codigo);
    }
}