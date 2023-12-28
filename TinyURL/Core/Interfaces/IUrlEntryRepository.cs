using System.Collections.Generic;
using System.Threading.Tasks;
using TinyURL.Core.Models;

namespace TinyURL.Core.Interfaces
{
    public interface IUrlEntryRepository
    {
        Task<IEnumerable<UrlEntry>> GetAllUrlsAsync();
        Task<UrlEntry> GetUrlByShortUrlAsync(string shortUrl);
        Task<UrlEntry> GetUrlByOriginalUrlAsync(string originalUrl);
        Task<long> GetNextSequenceValueAsync();

        Task AddUrlAsync(UrlEntry urlEntry);
        Task UpdateUrlAsync(string id, UrlEntry urlEntry);
        Task DeleteUrlAsync(string id);
    }
}
