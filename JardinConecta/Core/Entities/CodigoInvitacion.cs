namespace JardinConecta.Core.Entities
{
    public class CodigoInvitacion
    {
        public Guid Id { get; set; }
        public string Codigo { get; set; } = null!;
        public Guid IdSala { get; set; }
        public Guid? IdInfante { get; set; }
        public int TipoInvitacion { get; set; }
        public DateTime FechaExpiracion { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public virtual Sala Sala { get; set; } = null!;
        public virtual Infante? Infante { get; set; }
    }
}
