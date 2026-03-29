namespace JardinConecta.Core.Services.Dtos;

public record InfanteSalaResult(
    Guid IdSala,
    string Nombre
);

public record InfanteResult(
    Guid Id,
    string Nombre,
    string Apellido,
    string? Documento,
    string? PhotoUrl,
    DateTime FechaNacimiento,
    List<InfanteSalaResult> Salas
);

public record InfanteTutelaResult(
    Guid IdUsuario,
    string NombreUsuario,
    string ApellidoUsuario,
    string TipoTutela
);

public record InfanteDetalleResult(
    Guid Id,
    string Nombre,
    string Apellido,
    string? Documento,
    string? PhotoUrl,
    DateTime FechaNacimiento,
    List<InfanteTutelaResult> Tutelas,
    List<InfanteSalaResult> Salas
);
