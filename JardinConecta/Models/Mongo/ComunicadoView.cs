using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace JardinConecta.Models.Mongo
{
    public class ComunicadoView
    {
        public const string COLLECTION_NAME = "comunicados_view";

        // MongoDB ObjectId
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        // Reference to comunicado
        [BsonElement("comunicadoId")]
        [BsonRepresentation(BsonType.String)]
        public Guid ComunicadoId { get; set; }

        // Who viewed
        [BsonElement("userId")]
        [BsonRepresentation(BsonType.String)]
        public Guid UserId { get; set; }

        // When viewed
        [BsonElement("viewedAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime ViewedAt { get; set; } = DateTime.UtcNow;
    }
}
