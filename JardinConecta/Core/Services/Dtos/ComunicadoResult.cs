namespace JardinConecta.Core.Services.Dtos;

public record ComunicadoItemResult(
    Guid Id,
    string Titulo,
    string Contenido,
    string Autor,
    int Views,
    DateTime FechaCreacion,
    int Estado,
    DateTime? FechaPublicacion,
    DateTime? FechaPrograma
);

public record ComunicadoDetalleResult(
    Guid Id,
    string Titulo,
    string Contenido,
    string Autor,
    int Views,
    DateTime FechaCreacion,
    ICollection<ComunicadoArchivoResult> Archivos,
    int Estado,
    DateTime? FechaPrograma,
    DateTime? UpdatedAt
);

public record ComunicadoArchivoResult(
    Guid Id,
    string Url,
    string Filename,
    string ContentType
);
