namespace JardinConecta.Models.Entities
{
    public class Sala
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = null!;
        public Guid IdJardin { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        //Navigation
        public virtual Jardin Jardin { get; set; } = null!;
        public virtual ICollection<UsuarioSalaRol> UsuariosSalasRoles { get; set; } = null!;
    }
}
