namespace JardinConecta.Models.Http.Responses
{
    public record SalaMiembroResponse(
        Guid IdUsuario,
        string Nombre,
        string Apellido,
        string Rol,
        List<TutelaInfo> Tutelas
    );

    public record TutelaInfo(
        Guid IdInfante,
        string NombreInfante,
        string ApellidoInfante,
        string TipoTutela
    );
}
