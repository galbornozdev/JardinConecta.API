namespace JardinConecta.Http.Responses
{
    public record UsuarioLogueadoResponse(
        string Email,
        string? Nombre,
        string? Apellido,
        string? Documento,
        string? PhotoUrl,
        IList<UsuarioLogueadoResponse_Jardin> Jardines
    );
    public record UsuarioLogueadoResponse_Jardin(Guid IdJardin, string? Nombre, bool EsEducador);
}
