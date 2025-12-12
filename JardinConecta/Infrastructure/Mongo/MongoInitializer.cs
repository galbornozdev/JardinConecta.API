using JardinConecta.Models.Mongo;
using MongoDB.Driver;

namespace JardinConecta.Infrastructure.Mongo
{
    public static class MongoInitializer
    {
        public static async Task InitializeAsync(IMongoDatabase db)
        {
            var comunicados = db.GetCollection<Comunicado>(Comunicado.COLLECTION_NAME);

            // Example: index on SalaId (recommended)
            var indexSalaId = Builders<Comunicado>.IndexKeys.Ascending(x => x.SalaId);
            await comunicados.Indexes.CreateOneAsync(
                new CreateIndexModel<Comunicado>(indexSalaId)
            );

            // Example: index on createdAt
            var indexCreated = Builders<Comunicado>.IndexKeys.Descending(x => x.CreatedAt);
            await comunicados.Indexes.CreateOneAsync(
                new CreateIndexModel<Comunicado>(indexCreated)
            );

            // Example: compound index
            // await comunicados.Indexes.CreateOneAsync(
            //     new CreateIndexModel<Comunicado>(
            //         Builders<Comunicado>.IndexKeys
            //             .Ascending(x => x.SalaId)
            //             .Descending(x => x.CreatedAt)
            //     )
            // );
        }
    }
}
