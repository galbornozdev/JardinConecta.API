namespace JardinConecta.Http.Requests
{
    public class AltaComunicadoRequest
    {
        public Guid SalaId { get; set; }
        public string Title { get; set; } = null!;
        public string Text { get; set; } = null!;
    }
}