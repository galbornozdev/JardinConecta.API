namespace JardinConecta.Models.Entities
{
    public class Comunicado
    {
        public Guid Id { get; set; }
        public Guid IdSala { get; set; }
        public Guid IdUsuario { get; set; }
        public string Titulo { get; set; } = null!;
        public string Contenido { get; set; } = null!;
        virtual public ICollection<ComunicadoView> ComunicadoViews { get; set; } = new List<ComunicadoView>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public virtual Usuario Usuario { get; set; } = null!;
    }
}
