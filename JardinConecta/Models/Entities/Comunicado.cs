namespace JardinConecta.Models.Entities
{
    public class Comunicado
    {
        public Guid Id { get; set; }
        public string Titulo { get; set; }
        public string Contenido { get; set; }
        public Guid IdUsuarioRemitente { get; set; }
        public Guid IdSala { get; set; }

        //Navigation
        public virtual Sala Sala { get; set; } = null!;
        public virtual Usuario UsuarioRemitente { get; set; } = null!;
        public virtual ICollection<Usuario> UsuariosDestinatarios { get; set; } = [];
    }
}
