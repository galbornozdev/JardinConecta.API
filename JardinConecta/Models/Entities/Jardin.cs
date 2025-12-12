namespace JardinConecta.Models.Entities
{
    public class Jardin
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        //Navigation
        public virtual ICollection<Sala> Salas { get; set; } = [];
        public virtual ICollection<Infante> Infantes { get; set; } = [];
    }
}
