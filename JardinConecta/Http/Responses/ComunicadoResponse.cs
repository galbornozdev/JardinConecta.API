namespace JardinConecta.Http.Responses
{
    public record ComunicadoResponse(Guid Id, string Titulo, string Contenido, string Autor, int Views, DateTime FechaCreacion, ICollection<ComunicadoArchivoResponse> Archivos);

    public record ComunicadoArchivoResponse(string Url, string Filename, string ContentType);
}
