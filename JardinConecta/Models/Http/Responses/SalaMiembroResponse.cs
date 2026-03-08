namespace JardinConecta.Models.Http.Responses
{
    public record SalaMiembroResponse(
        Guid IdUsuario,
        string Nombre,
        string Apellido,
        string Rol
    );
}
