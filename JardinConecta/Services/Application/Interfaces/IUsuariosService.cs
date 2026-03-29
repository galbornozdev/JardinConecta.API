using JardinConecta.Models.Http.Responses;

namespace JardinConecta.Services.Application.Interfaces
{
    public interface IUsuariosService
    {
        Task ActualizarDeviceToken(Guid idUsuario, string deviceToken);
        Task<string> ActualizarFotoPerfil(Guid idUsuario, IFormFile fotoPerfil);
        Task ActualizarInformacionPersonal(Guid idUsuario, string nombre, string apellido, string? documento = null);
        Task AltaDeUsuario(string email, string password);
        Task<UsuarioLogueadoResponse> ObtenerUsuario(Guid idUsuario);
        Task<bool> VerificarEmail(string token);
    }
}