namespace JardinConecta.Models.Http.Responses
{
    public record TutelaDetalleResponse(string TipoTutela, string NombreInfante);

    public record ComunicadoViewDetalleResponse(
        string NombreCompleto,
        DateTime ViewedAt,
        IList<TutelaDetalleResponse> Tutelas
    );
}
