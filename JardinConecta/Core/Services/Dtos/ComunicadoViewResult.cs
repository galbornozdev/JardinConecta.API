namespace JardinConecta.Core.Services.Dtos;

public record TutelaDetalleResult(
    string TipoTutela,
    string NombreInfante
);

public record ComunicadoViewDetalleResult(
    string NombreCompleto,
    DateTime ViewedAt,
    string? PhotoUrl,
    IList<TutelaDetalleResult> Tutelas
);
