namespace JardinConecta.Http.Responses
{
    public record ComunicadoItemResponse(Guid Id, string Titulo, string Contenido, string Autor, int Views, DateTime FechaCreacion);
}
