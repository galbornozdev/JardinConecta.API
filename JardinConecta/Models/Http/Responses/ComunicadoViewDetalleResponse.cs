namespace JardinConecta.Models.Http.Responses
{
    public record ComunicadoViewDetalleResponse(
        string NombreCompleto,
        string TipoTutela,
        string NombreInfante,
        DateTime ViewedAt
    );
}
