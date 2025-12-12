namespace JardinConecta.Models.Entities
{
    public class Infante
    {
        public Guid Id { get; set; }
        public Guid IdJardin { get; set; }
        public string Nombre { get; set; } = null!;
        public string Apellido { get; set; } = null!;
        public string? Documento { get; set; }
        public string? PhotoUrl { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        //Navigation
        public virtual Jardin Jardin { get; set; } = null!;
        public virtual ICollection<Tutela> Tutelas { get; set; } = null!;
    }
}
