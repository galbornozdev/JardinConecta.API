using JardinConecta.Core.Entities;
using JardinConecta.Core.Services.Dtos;

namespace JardinConecta.Core.Interfaces
{
    public interface IChatService
    {
        Task<ChatMensajeResult> EnviarMensaje(Guid IdUsuarioLogueado, Guid IdUsuarioContraparte, Guid idSala, string texto);
        Task MarcarMensajesComoLeidos(Guid IdUsuarioLogueado, Guid IdUsuarioContraparte, Guid idSala);
        Task<List<ContactoChatResult>> ObtenerContactos(Guid IdUsuarioLogueado, Guid idSala, TipoUsuarioId tipoUsuario);
        Task<List<ConversacionItemResult>> ObtenerConversaciones(Guid IdUsuario);
        Task<PagedResult<ChatMensajeResult>> ObtenerMensajes(Guid IdUsuarioLogueado, Guid IdUsuarioContraparte, Guid idSala, int page = 1, int pageSize = 20);
    }
}
