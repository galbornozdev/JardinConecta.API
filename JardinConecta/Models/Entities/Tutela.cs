namespace JardinConecta.Models.Entities
{
    public class Tutela
    {
        public Guid IdInfante { get; set; }
        public Guid IdUsuario { get; set; }
        public int IdTipoTutela { get; set; }
        public DateTime CreatedAt { get; set; }

        //Navigation
        public virtual TipoTutela TipoTutela { get; set; } = null!;
        public virtual Infante Infante { get; set; } = null!;
        public virtual Usuario Usuario { get; set; } = null!;
    }
}
