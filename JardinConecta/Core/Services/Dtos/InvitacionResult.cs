namespace JardinConecta.Core.Services.Dtos;

public record CodigoInvitacionResult(
    Guid Id,
    string Codigo,
    Guid IdSala,
    Guid? IdInfante,
    int TipoInvitacion,
    DateTime FechaExpiracion
);

public record CodigoInvitacionItemResult(
    Guid Id,
    string Codigo,
    string? NombreInfante,
    int TipoInvitacion,
    DateTime FechaExpiracion,
    bool EstaVigente
);

public record VerificarInvitacionResult(
    string TipoInvitacion,
    string NombreSala,
    string NombreJardin
);
