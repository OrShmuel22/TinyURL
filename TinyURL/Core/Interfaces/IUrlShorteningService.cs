using TinyURL.Core.Models;

namespace TinyURL.Core.Interfaces
{
    public interface IUrlShorteningService
    {
        Task<UrlMapping> ShortenUrlAsync(string originalUrl);
        Task<string> ExpandUrlAsync(string shortUrl);
    }
}
