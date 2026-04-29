namespace JardinConecta.Core.Entities
{
    public class CodigoInvitacionInfante
    {
        public Guid IdCodigoInvitacion { get; set; }
        public Guid IdInfante { get; set; }

        public virtual CodigoInvitacion CodigoInvitacion { get; set; } = null!;
        public virtual Infante Infante { get; set; } = null!;
    }
}
