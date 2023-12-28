using MongoDB.Bson;
using MongoDB.Driver;
using TinyURL.Core.Interfaces;
using TinyURL.Core.Models;
using TinyURL.Data.Context;


namespace TinyURL.Data.Repositories
{
    public class UrlEntryRepository : IUrlEntryRepository
    {
        private readonly IMongoCollection<UrlEntry> _urls;
        private readonly ILogger<UrlEntryRepository> _logger;
        private readonly IMongoCollection<BsonDocument> _sequence;
        private const long InitialValue = 10000000;
        private const long MaxValue = 99999999;
        private const string SequenceId = "url_sequence_id";

        public UrlEntryRepository(MongoDbContext dbContext, ILogger<UrlEntryRepository> logger)
        {
            _urls = dbContext.UrlEntries;
            _sequence = dbContext.UrlSequence;
            _logger = logger;
        }

        public async Task<IEnumerable<UrlEntry>> GetAllUrlsAsync()
        {
            try
            {
                return await _urls.Find(_ => true).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while fetching all URLs: {ex.Message}");
                throw;
            }
        }

        public async Task<UrlEntry> GetUrlByShortUrlAsync(string shortUrl)
        {
            try
            {
                return await _urls.Find(url => url.ShortUrl == shortUrl).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while fetching the URL for shortUrl {shortUrl}: {ex.Message}");
                throw;
            }
        }

        public async Task AddUrlAsync(UrlEntry urlEntry)
        {
            try
            {
                await _urls.InsertOneAsync(urlEntry);
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while adding a new URL: {ex.Message}");
                throw;
            }
        }

        public async Task UpdateUrlAsync(string id, UrlEntry urlEntry)
        {
            try
            {
                await _urls.ReplaceOneAsync(url => url.Id == id, urlEntry);
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while updating the URL with id {id}: {ex.Message}");
                throw;
            }
        }

        public async Task<UrlEntry> GetUrlByOriginalUrlAsync(string originalUrl)
        {
            try
            {
                // Query the database for a UrlEntry with the specified originalUrl
                return await _urls.Find(url => url.OriginalUrl == originalUrl).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while fetching the URL for originalUrl {originalUrl}: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteUrlAsync(string id)
        {
            try
            {
                await _urls.DeleteOneAsync(url => url.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while deleting the URL with id {id}: {ex.Message}");
                throw;
            }
        }
        public async Task<long> GetNextSequenceValueAsync()
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("_id", SequenceId);

                // Attempt to increment the sequence value
                var update = Builders<BsonDocument>.Update.Inc("sequence_value", 1);
                var options = new FindOneAndUpdateOptions<BsonDocument, BsonDocument>
                {
                    ReturnDocument = ReturnDocument.After
                };

                var updatedDocument = await _sequence.FindOneAndUpdateAsync(filter, update, options);

                // If the document doesn't exist, create it
                if (updatedDocument == null)
                {
                    var initialDoc = new BsonDocument
            {
                { "_id", SequenceId },
                { "sequence_value", InitialValue }
            };
                    await _sequence.InsertOneAsync(initialDoc);
                    return InitialValue;
                }

                long sequenceValue = updatedDocument["sequence_value"].AsInt64;

                // Check if the sequence value has reached its upper limit
                if (sequenceValue > MaxValue)
                {
                    await ResetSequenceAsync();
                    return InitialValue;
                }

                return sequenceValue;
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while incrementing the sequence value: {ex}");
                throw;
            }
        }

        private async Task ResetSequenceAsync()
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", SequenceId);
            var resetUpdate = Builders<BsonDocument>.Update.Set("sequence_value", InitialValue);
            await _sequence.UpdateOneAsync(filter, resetUpdate);
        }

    }
}
