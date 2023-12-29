using MongoDB.Bson;
using MongoDB.Driver;
using System.Xml.Serialization;
using TinyURL.Core.Interfaces;
using TinyURL.Core.Models;
using TinyURL.Data.Context;


namespace TinyURL.Data.Repositories
{
    public class UrlEntryRepository : IUrlEntryRepository
    {
        private readonly IMongoCollection<urlMapping> _urls;
        private readonly ILogger<UrlEntryRepository> _logger;
        private readonly IMongoCollection<BsonDocument> _counters;
        private const string CounterId = "urlId";

        public UrlEntryRepository(MongoDbContext dbContext, ILogger<UrlEntryRepository> logger)
        {
            _urls = dbContext.urlMapping;
            _counters = dbContext.Counters;
            _logger = logger;
        }

        public async Task<IEnumerable<urlMapping>> GetAllUrlsAsync()
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

        public async Task<urlMapping> GetUrlByShortUrlAsync(string shortUrl)
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

        public async Task AddUrlAsync(urlMapping urlEntry)
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

        public async Task UpdateUrlAsync(string id, urlMapping urlEntry)
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

        public async Task<urlMapping> GetUrlByOriginalUrlAsync(string originalUrl)
        {
            try
            {
                // Asynchronously search for the UrlEntry with the specified originalUrl
                var urlMappingResult = await _urls.Find(url => url.OriginalUrl == originalUrl).FirstOrDefaultAsync();
                return urlMappingResult;
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

        public async Task<long> GetNextIdAsync()
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("_id", CounterId);
                var update = Builders<BsonDocument>.Update.Inc("sequence_value", 1);
                var options = new FindOneAndUpdateOptions<BsonDocument, BsonDocument>
                {
                    ReturnDocument = ReturnDocument.After,
                    IsUpsert = true
                };

                var result = await _counters.FindOneAndUpdateAsync(filter, update, options);
                return result["sequence_value"].AsInt32;
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while fetching the next ID: {ex.Message}");
                throw;
            }
        }
    }
}
