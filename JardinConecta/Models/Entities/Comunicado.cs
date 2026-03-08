namespace JardinConecta.Models.Entities
{
    public enum EstadoComunicado
    {
        Borrador = 1,
        Publicado = 2,
        Programado = 3
    }

    public class Comunicado
    {
        public Guid Id { get; set; }
        public Guid IdSala { get; set; }
        public Guid IdUsuario { get; set; }
        public string Titulo { get; set; } = null!;
        public string Contenido { get; set; } = null!;
        public string ContenidoTextoPlano { get; set; } = null!;
        public int Estado { get; set; } = (int)EstadoComunicado.Publicado;
        public DateTime? FechaPrograma { get; set; }
        virtual public ICollection<ComunicadoView> Views { get; set; } = new List<ComunicadoView>();
        virtual public ICollection<ComunicadoArchivo> Archivos { get; set; } = new List<ComunicadoArchivo>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public virtual Sala Sala { get; set; } = null!;
        public virtual Usuario Usuario { get; set; } = null!;
    }
}
