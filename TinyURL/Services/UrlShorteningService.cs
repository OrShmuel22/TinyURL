using Amazon.Runtime.Internal.Util;
using System;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using TinyURL.Core.Interfaces;
using TinyURL.Core.Models;

namespace TinyURL.Services
{
    public class UrlShorteningService : IUrlShorteningService
    {
        private readonly IUrlEntryRepository _urlEntryRepository;
        private readonly ICustomMemoryCache<UrlEntry> _cache;

        public UrlShorteningService(IUrlEntryRepository urlEntryRepository, ICustomMemoryCache<UrlEntry> cache)
        {
            _urlEntryRepository = urlEntryRepository;
            _cache = cache;

        }

        public async Task<UrlEntry> ShortenUrlAsync(string originalUrl)
        {
            UrlEntry urlEntry = null;
            if(!_cache.TryGetValue(originalUrl, out urlEntry))
            {
                var shortUrl = await GenerateShortUrl();
                urlEntry.OriginalUrl = originalUrl;
                urlEntry.ShortUrl = shortUrl;
                _cache.Add(originalUrl, urlEntry);
            }
            await _urlEntryRepository.AddUrlAsync(urlEntry);
            return urlEntry;
        }

        public async Task<string> ExpandUrlAsync(string shortUrl)
        {
            var urlEntry = await _urlEntryRepository.GetUrlByShortUrlAsync(shortUrl);
            return urlEntry?.OriginalUrl;
        }

        private async Task<string> GenerateShortUrl()
        {
            long sequenceValue = await _urlEntryRepository.GetNextSequenceValueAsync();
            return Base62Encode(sequenceValue);
        }

        private string Base62Encode(long value)
        {
            const string characters = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var sb = new StringBuilder();

            while (value > 0)
            {
                sb.Insert(0, characters[(int)(value % 62)]);
                value /= 62;
            }

            string encoded = sb.ToString();
            if (encoded.Length > 8)
            {
                throw new InvalidOperationException("Encoded string exceeds 8 characters.");
            }

            return encoded.PadLeft(8, '0');
        }

    }
}
