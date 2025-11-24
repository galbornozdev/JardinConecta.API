namespace JardinConecta.Models.Entities
{
    public class UsuarioSalaRol
    {
        public Guid IdUsuario { get; set; }
        public Guid IdSala { get; set; }
        public int IdRolSala { get; set; }

        //Navigation
        public virtual Usuario Usuario { get; set; } = null!;
        public virtual Sala Sala { get; set; } = null!;
        public virtual RolSala RolSala { get; set; } = null!;
    }
}
