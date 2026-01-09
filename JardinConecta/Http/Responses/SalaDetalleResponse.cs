namespace JardinConecta.Http.Responses
{
    public record SalaDetalleResponse(Guid Id, string Nombre, ICollection<SalaDetalleResponse_UsuariosMiembros> UsuariosMiembros);
    public record SalaDetalleResponse_UsuariosMiembros(Guid Id, string Nombre, string Apellido);
}
