namespace JardinConecta.Models.Http.Responses
{
    public record ConversacionItemResponse(
        Guid IdUsuario,
        string NombreCompleto,
        string? PhotoUrl,
        string UltimoMensaje,
        DateTime FechaUltimoMensaje,
        int MensajesNoLeidos,
        Guid IdSala,
        string NombreSala
    );
}
