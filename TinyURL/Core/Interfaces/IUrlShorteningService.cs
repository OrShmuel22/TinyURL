using TinyURL.Core.Models;
using System.Threading.Tasks;

namespace TinyURL.Core.Interfaces
{
    public interface IUrlShorteningService
    {
        Task<UrlEntry> ShortenUrlAsync(string originalUrl);
        Task<string> ExpandUrlAsync(string shortUrl);
    }
}
