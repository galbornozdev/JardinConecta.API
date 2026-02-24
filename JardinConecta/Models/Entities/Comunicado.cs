namespace JardinConecta.Models.Entities
{
    public class Comunicado
    {
        public Guid Id { get; set; }
        public Guid IdSala { get; set; }
        public Guid IdUsuario { get; set; }
        public string Titulo { get; set; } = null!;
        public string Contenido { get; set; } = null!;
        public string ContenidoTextoPlano { get; set; } = null!;
        virtual public ICollection<ComunicadoView> ComunicadoViews { get; set; } = new List<ComunicadoView>();
        virtual public ICollection<ComunicadoArchivo> ComunicadoArchivos { get; set; } = new List<ComunicadoArchivo>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public virtual Sala Sala { get; set; } = null!;
        public virtual Usuario Usuario { get; set; } = null!;
    }
}
