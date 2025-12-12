using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace JardinConecta.Models.Mongo
{
    public class Comunicado
    {
        public const string COLLECTION_NAME = "comunicados";

        // MongoDB ObjectId
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("salaId")]
        public string SalaId { get; set; } = null!;

        [BsonElement("senderId")]
        public string SenderId { get; set; } = null!;

        [BsonElement("title")]
        public string Title { get; set; } = null!;

        // Message text
        [BsonElement("text")]
        public string Text { get; set; } = null!;

        // Timestamp
        [BsonElement("createdAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Timestamp
        [BsonElement("updatedAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
