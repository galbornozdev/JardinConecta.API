namespace JardinConecta.Models.Http.Requests
{
    public class EditarComunicadoRequest
    {
        public string Titulo { get; set; } = null!;
        public string Contenido { get; set; } = null!;
        public string ContenidoTextoPlano { get; set; } = null!;
        public int Estado { get; set; }
        public DateTime? FechaPrograma { get; set; }
        public List<Guid>? ArchivosEliminar { get; set; }
        public List<IFormFile>? Archivos { get; set; }
    }
}
