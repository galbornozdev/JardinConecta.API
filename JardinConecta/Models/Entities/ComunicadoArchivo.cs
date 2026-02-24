namespace JardinConecta.Models.Entities
{
    public class ComunicadoArchivo
    {
        public Guid Id { get; set; }
        public Guid IdComunicado { get; set; }
        public string NombreArchivoOriginal { get; set; } = null!;
        public string ContentType { get; set; } = null!;
        public string Extension { get; set; } = null!;

        // Relaciones
        public Comunicado Comunicado { get; set; } = null!;
    }
}
