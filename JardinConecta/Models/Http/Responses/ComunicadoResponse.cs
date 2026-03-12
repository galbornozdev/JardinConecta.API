namespace JardinConecta.Models.Http.Responses
{
    public record ComunicadoResponse(Guid Id, string Titulo, string Contenido, string Autor, int Views, DateTime FechaCreacion, ICollection<ComunicadoArchivoResponse> Archivos, int Estado, DateTime? FechaPrograma, DateTime? UpdatedAt);

    public record ComunicadoArchivoResponse(Guid Id, string Url, string Filename, string ContentType);
}
