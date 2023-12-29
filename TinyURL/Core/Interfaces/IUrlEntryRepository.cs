using TinyURL.Core.Models;

namespace TinyURL.Core.Interfaces
{
    public interface IUrlEntryRepository
    {
        Task<IEnumerable<urlMapping>> GetAllUrlsAsync();
        Task<urlMapping> GetUrlByShortUrlAsync(string shortUrl);
        Task<urlMapping> GetUrlByOriginalUrlAsync(string originalUrl);
        Task<long> GetNextIdAsync();

        Task AddUrlAsync(urlMapping urlEntry);
        Task UpdateUrlAsync(string id, urlMapping urlEntry);
        Task DeleteUrlAsync(string id);
    }
}
