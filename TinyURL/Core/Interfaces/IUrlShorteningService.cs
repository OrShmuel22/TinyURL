using TinyURL.Core.Models;

namespace TinyURL.Core.Interfaces
{
    public interface IUrlShorteningService
    {
        Task<urlMapping> ShortenUrlAsync(string originalUrl);
        Task<string> ExpandUrlAsync(string shortUrl);
    }
}
