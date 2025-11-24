namespace JardinConecta.Models.Entities
{
    public class Jardin
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = null!;

        //Navigation
        public virtual ICollection<Sala> Salas { get; set; } = [];
    }
}
