namespace JardinConecta.Models.Entities
{
    public class Persona
    {
        public Guid IdUsuario { get; set; }
        public string Nombre { get; set; } = null!;
        public string Apellido { get; set; } = null!;
        public string? Documento { get; set; }
        public string? PhotoUrl { get; set; }

        //Navigation
        public virtual Usuario Usuario { get; set; } = null!;
    }
}
