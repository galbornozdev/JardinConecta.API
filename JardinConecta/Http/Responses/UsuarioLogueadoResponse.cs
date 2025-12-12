namespace JardinConecta.Http.Responses
{
    public class UsuarioLogueadoResponse
    {
        public string Email { get; set; } = null!;
        public string? Nombre { get; set; } = null!;
        public string? Apellido { get; set; } = null!;
        public string? Documento { get; set; }
        public string? PhotoUrl { get; set; }
    }
}
