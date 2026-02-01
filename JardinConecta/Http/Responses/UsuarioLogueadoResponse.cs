namespace JardinConecta.Http.Responses
{
    public record UsuarioLogueadoResponse(
        string Email,
        string? Nombre,
        string? Apellido,
        string? Documento,
        string? PhotoUrl,
        ICollection<UsuarioLogueadoResponse_Jardin> Jardines,
        ICollection<UsuarioLogueadoResponse_Sala> Salas
    );
    public record UsuarioLogueadoResponse_Jardin(Guid Id, string? Nombre);
    public record UsuarioLogueadoResponse_Sala(Guid Id, Guid IdJardin, string? Nombre, bool EsEducador);
}
