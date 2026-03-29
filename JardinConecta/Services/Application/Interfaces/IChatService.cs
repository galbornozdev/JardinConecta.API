using JardinConecta.Models.Http.Responses;

namespace JardinConecta.Services.Application.Interfaces
{
    public interface IChatService
    {
        Task<ChatMensajeResponse> EnviarMensaje(Guid IdUsuarioLogueado, Guid IdUsuarioContraparte, Guid idSala, string texto);
        Task MarcarMensajesComoLeidos(Guid IdUsuarioLogueado, Guid IdUsuarioContraparte, Guid idSala);
        Task<List<ContactoChatResponse>> ObtenerContactos(Guid IdUsuarioLogueado, Guid idSala);
        Task<List<ConversacionItemResponse>> ObtenerConversaciones(Guid IdUsuario);
        Task<Pagination<ChatMensajeResponse>> ObtenerMensajes(Guid IdUsuarioLogueado, Guid IdUsuarioContraparte, Guid idSala, int page = 1, int pageSize = 20);
    }
}