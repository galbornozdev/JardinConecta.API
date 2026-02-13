namespace JardinConecta.Models.Entities
{
    public class ComunicadoView
    {
        public Guid IdComunicado { get; set; }
        public Guid IdUsuario { get; set; }
        public DateTime ViewedAt { get; set; } = DateTime.UtcNow;

        virtual public Comunicado Comunicado { get; set; } = null!;
    }
}
