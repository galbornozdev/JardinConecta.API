namespace JardinConecta.Models.Http.Responses
{
    public record ConversacionItemResponse(
        Guid IdUsuario,
        string NombreCompleto,
        string? FotoUrl,
        string UltimoMensaje,
        DateTime FechaUltimoMensaje,
        int MensajesNoLeidos,
        Guid IdSala,
        string NombreSala
    );
}
