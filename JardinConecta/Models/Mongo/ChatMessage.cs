using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace JardinConecta.Models.Mongo
{
    public class ChatMessage
    {
        // MongoDB ObjectId
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        // Chat this message belongs to
        [BsonElement("chatId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ChatId { get; set; } = null!;

        // Sender user ID
        [BsonElement("senderId")]
        public string SenderId { get; set; } = null!;

        // Message text
        [BsonElement("text")]
        public string Text { get; set; } = null!;

        // Timestamp
        [BsonElement("createdAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Read status
        [BsonElement("read")]
        public bool Read { get; set; } = false;
    }
}
