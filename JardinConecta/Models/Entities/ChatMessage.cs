namespace JardinConecta.Models.Entities
{
    public class ChatMessage
    {
        // MongoDB ObjectId
        public string? Id { get; set; }

        // Chat this message belongs to
        public string ChatId { get; set; } = null!;

        // Sender user ID
        public string SenderId { get; set; } = null!;

        // Message text
        public string Text { get; set; } = null!;

        // Timestamp
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Timestamp
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Read status
        public bool Read { get; set; } = false;
    }
}
