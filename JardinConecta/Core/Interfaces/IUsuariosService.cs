using JardinConecta.Core.Entities;
using JardinConecta.Core.Services.Dtos;

namespace JardinConecta.Core.Interfaces
{
    public interface IUsuariosService
    {
        Task ActualizarDeviceToken(Guid idUsuario, string deviceToken);
        Task<string> ActualizarFotoPerfil(Guid idUsuario, IFormFile fotoPerfil);
        Task ActualizarInformacionPersonal(Guid idUsuario, string nombre, string apellido, string? documento = null);
        Task AltaDeUsuario(string email, string password);
        Task<UsuarioLogueadoResult> ObtenerUsuarioLogueado(Guid idUsuario, TipoUsuarioId tipoUsuarioId, Guid? idJardin);
        Task<bool> VerificarEmail(string token);
    }
}
