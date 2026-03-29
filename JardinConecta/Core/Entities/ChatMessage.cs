namespace JardinConecta.Core.Entities
{
    public class ChatMessage
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid IdRemitente { get; set; }
        public Guid IdDestinatario { get; set; }
        public Guid? IdSala { get; set; }
        public string Texto { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LeidoAt { get; set; }

        public Usuario Remitente { get; set; } = null!;
        public Usuario Destinatario { get; set; } = null!;
        public Sala? Sala { get; set; }
    }
}
