using TinyURL.Core.Models;
using System.Threading.Tasks;

namespace TinyURL.Core.Interfaces
{
    public interface IUrlShorteningService
    {
        Task<urlMapping> ShortenUrlAsync(string originalUrl);
        Task<string> ExpandUrlAsync(string shortUrl);
    }
}
