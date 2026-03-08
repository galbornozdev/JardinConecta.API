namespace JardinConecta.Models.Http.Requests
{
    public class AltaComunicadoRequest
    {
        public Guid IdSala { get; set; }
        public string Titulo { get; set; } = null!;
        public string Contenido { get; set; } = null!;
        public string ContenidoTextoPlano { get; set; } = null!;
        public int Estado { get; set; } = 2; // 2 = Publicado por defecto
        public DateTime? FechaPrograma { get; set; }
        // public bool HabilitarAprobarRechazar { get; set; } // TODO: Feature pendiente
        public List<IFormFile> Archivos { get; set; } = new List<IFormFile>();
    }
}