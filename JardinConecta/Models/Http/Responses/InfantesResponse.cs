namespace JardinConecta.Models.Http.Responses
{
    public record InfantesResponse(
        Guid Id,
        string Nombre,
        string Apellido,
        string? Documento,
        string? PhotoUrl,
        DateTime FechaNacimiento,
        List<InfantesResponse_Sala> Salas
    );

    public record InfantesResponse_Sala(Guid IdSala, string Nombre);
}
