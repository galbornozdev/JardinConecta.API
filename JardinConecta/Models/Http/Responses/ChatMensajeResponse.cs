namespace JardinConecta.Models.Http.Responses
{
    public record ChatMensajeResponse(
        Guid Id,
        Guid IdRemitente,
        string Texto,
        DateTime CreatedAt,
        bool Leido
    );
}
