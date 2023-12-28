using MongoDB.Bson;
using MongoDB.Driver;
using TinyURL.Core.Models;


namespace TinyURL.Data.Context
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;
        private readonly ILogger<MongoDbContext> _logger;
        public IMongoCollection<BsonDocument> UrlSequence { get; private set; }

        public MongoDbContext(MongoDBSettings settings, ILogger<MongoDbContext> logger)
        {
            _logger = logger;
            try
            {
                var client = new MongoClient(settings.ConnectionString);
                _database = client.GetDatabase(settings.DatabaseName);
                UrlSequence = client.GetDatabase(settings.DatabaseName).GetCollection<BsonDocument>(settings.UrlSequenceCollection);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing MongoDB connection");
                throw;
            }
        }

        public IMongoCollection<UrlEntry> UrlEntries => _database.GetCollection<UrlEntry>("UrlEntries");
    }
}
