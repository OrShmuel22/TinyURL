using TinyURL.Core.Models;

namespace TinyURL.Core.Interfaces
{
    public interface IUrlEntryRepository
    {
        Task<IEnumerable<UrlMapping>> GetAllUrlsAsync();
        Task<UrlMapping> GetUrlByShortUrlAsync(string shortUrl);
        Task<UrlMapping> GetUrlByOriginalUrlAsync(string originalUrl);
        Task<long> GetNextIdAsync();

        Task AddUrlAsync(UrlMapping urlEntry);
        Task UpdateUrlAsync(string id, UrlMapping urlEntry);
        Task DeleteUrlAsync(string id);
    }
}
