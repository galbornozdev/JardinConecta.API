using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace JardinConecta.Models.Mongo
{
    public class Comunicado
    {
        public const string COLLECTION_NAME = "comunicados";

        // MongoDB ObjectId
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }

        [BsonElement("salaId")]
        [BsonRepresentation(BsonType.String)]
        public Guid SalaId { get; set; }

        [BsonElement("senderId")]
        [BsonRepresentation(BsonType.String)]
        public Guid SenderId { get; set; }

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
