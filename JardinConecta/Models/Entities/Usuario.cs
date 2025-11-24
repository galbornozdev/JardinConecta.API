namespace JardinConecta.Models.Entities
{
    public class Usuario
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public virtual Telefono Telefono { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        
        public int IdRol { get; set; }
        public Guid? IdJardin { get; set; }

        //Navigation
        public virtual Persona? Persona { get; set; }
        public virtual Rol Rol { get; set; } = null!;
        public virtual Jardin? Jardin { get; set; }

        public virtual ICollection<Tutela> Tutelas { get; set; } = null!;
        public virtual ICollection<UsuarioSalaRol> UsuariosSalasRoles { get; set; } = null!;
    }
}