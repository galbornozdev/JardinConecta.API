namespace JardinConecta.Models.Http.Responses
{
    public record InfanteDetalleResponse(
        Guid Id,
        string Nombre,
        string Apellido,
        string? Documento,
        string? PhotoUrl,
        DateTime FechaNacimiento,
        List<InfanteDetalleResponse_Tutela> Tutelas,
        List<InfantesResponse_Sala> Salas
    );

    public record InfanteDetalleResponse_Tutela(
        Guid IdUsuario,
        string NombreUsuario,
        string ApellidoUsuario,
        string TipoTutela
    );
}
