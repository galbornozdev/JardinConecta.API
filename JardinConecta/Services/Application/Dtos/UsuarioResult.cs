namespace JardinConecta.Services.Application.Dtos;

public record UsuarioJardinResult(
    Guid Id,
    string? Nombre
);

public record UsuarioSalaResult(
    Guid Id,
    Guid IdJardin,
    string? Nombre,
    bool EsEducador
);

public record UsuarioLogueadoResult(
    Guid Id,
    string Email,
    string? Nombre,
    string? Apellido,
    string? Documento,
    string? PhotoUrl,
    ICollection<UsuarioJardinResult> Jardines,
    ICollection<UsuarioSalaResult> Salas
);
