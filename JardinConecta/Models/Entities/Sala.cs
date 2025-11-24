namespace JardinConecta.Models.Entities
{
    public class Sala
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = null!;
        public Guid IdJardin { get; set; }

        //Navigation
        public virtual Jardin Jardin { get; set; } = null!;
        public virtual ICollection<UsuarioSalaRol> UsuariosSalasRoles { get; set; } = null!;
    }
}
