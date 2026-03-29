namespace JardinConecta.Core.Services.Dtos;

public record ConversacionItemResult(
    Guid IdUsuario,
    string NombreCompleto,
    string? PhotoUrl,
    string UltimoMensaje,
    DateTime FechaUltimoMensaje,
    int MensajesNoLeidos,
    Guid IdSala,
    string NombreSala
);

public record ChatMensajeResult(
    Guid Id,
    Guid IdRemitente,
    string Texto,
    DateTime CreatedAt,
    bool Leido
);

public record ContactoChatResult(
    Guid IdUsuario,
    string NombreCompleto,
    string? PhotoUrl
);
