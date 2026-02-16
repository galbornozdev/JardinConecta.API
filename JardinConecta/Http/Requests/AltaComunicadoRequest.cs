namespace JardinConecta.Http.Requests
{
    public class AltaComunicadoRequest
    {
        public Guid IdSala { get; set; }
        public string Titulo { get; set; } = null!;
        public string Contenido { get; set; } = null!;
        public bool HabilitarAprobarRechazar { get; set; }
        public List<IFormFile> Archivos { get; set; } = new List<IFormFile>();
    }
}