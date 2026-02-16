namespace JardinConecta.Http.Responses
{
    public record ComunicadoResponse(Guid Id, string Titulo, string Contenido, string Autor, int Views, DateTime FechaCreacion);
}
