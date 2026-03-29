namespace JardinConecta.Core.Entities
{
    public class InfanteSala
    {
        public Guid IdInfante { get; set; }
        public Guid IdSala { get; set; }

        // Navigation
        public virtual Infante Infante { get; set; } = null!;
        public virtual Sala Sala { get; set; } = null!;
    }
}
