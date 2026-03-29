namespace JardinConecta.Core.Services.Dtos;

public record SalaResult(
    Guid Id,
    string Nombre
);

public record SalaMiembroBasicoResult(
    Guid Id,
    string Nombre,
    string Apellido
);

public record SalaDetalleResult(
    Guid Id,
    string Nombre,
    ICollection<SalaMiembroBasicoResult> Miembros
);

public record TutelaInfoResult(
    Guid IdInfante,
    string NombreInfante,
    string ApellidoInfante,
    string TipoTutela
);

public record SalaMiembroResult(
    Guid IdUsuario,
    string Nombre,
    string Apellido,
    string Rol,
    List<TutelaInfoResult> Tutelas
);
