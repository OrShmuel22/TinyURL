using TinyURL.Core.Interfaces;
using TinyURL.Core.Models;

namespace TinyURL.Services
{
    /// <summary>
    /// Service for URL shortening and expanding.
    /// </summary>
    public class UrlShorteningService : IUrlShorteningService
    {
        private readonly IUrlEntryRepository _urlEntryRepository;
        private readonly IBase62Encoding _base62;
        private readonly ICustomMemoryCache<string> _cache;
        private readonly UrlShorteningSettings _settings;

        public UrlShorteningService(UrlShorteningSettings settings, IUrlEntryRepository urlEntryRepository,
                                    ICustomMemoryCache<string> cache, IBase62Encoding base62)
        {
            _settings = settings;
            _urlEntryRepository = urlEntryRepository;
            _cache = cache;
            _base62 = base62;
        }

        /// <summary>
        /// Shortens a given URL.
        /// </summary>
        /// <param name="originalUrl">The original URL to be shortened.</param>
        /// <returns>The shortened URL mapping.</returns>
        public async Task<UrlMapping> ShortenUrlAsync(string originalUrl)
        {
            if (string.IsNullOrWhiteSpace(originalUrl) || !IsValidUrl(originalUrl))
                throw new ArgumentException("Original URL is not valid.", nameof(originalUrl));

            string cachedShortUrl;
            if (!_cache.TryGetValue(originalUrl, out cachedShortUrl))
            {
                UrlMapping urlEntry = await _urlEntryRepository.GetUrlByOriginalUrlAsync(originalUrl);
                if (urlEntry == null)
                {
                    string shortUrl = _settings.BaseUrl + await GenerateShortUrl();
                    urlEntry = new UrlMapping { OriginalUrl = originalUrl, ShortUrl = shortUrl };
                    await _urlEntryRepository.AddUrlAsync(urlEntry);
                    CacheUrls(originalUrl, shortUrl);
                }
                else
                    CacheUrls(originalUrl, urlEntry.ShortUrl);
                return urlEntry;
            }
            return new UrlMapping { OriginalUrl = originalUrl, ShortUrl = cachedShortUrl };
        }

        /// <summary>
        /// Expands a shortened URL.
        /// </summary>
        /// <param name="shortUrl">The shortened URL to be expanded.</param>
        /// <returns>The original URL.</returns>
        public async Task<string> ExpandUrlAsync(string shortUrl)
        {
            shortUrl = _settings.BaseUrl + shortUrl;
            if (string.IsNullOrWhiteSpace(shortUrl))
                throw new ArgumentException("Short URL cannot be null or whitespace.", nameof(shortUrl));

            string cachedOriginalUrl;
            if (!_cache.TryGetValue(shortUrl, out cachedOriginalUrl))
            {
                var urlEntry = await _urlEntryRepository.GetUrlByShortUrlAsync(shortUrl);
                if (urlEntry == null)
                    throw new InvalidOperationException("Short URL does not correspond to an existing original URL.");

                CacheUrls(shortUrl, urlEntry.OriginalUrl);
                return urlEntry.OriginalUrl;
            }
            return cachedOriginalUrl;
        }

        private async Task<string> GenerateShortUrl()
        {
            try
            {
                long urlId = await _urlEntryRepository.GetNextIdAsync();
                return _base62.Encode(urlId + _settings.BaseNumber.Value);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while generating the short URL.", ex);
            }
        }

        private bool IsValidUrl(string url)
        {
            return Uri.IsWellFormedUriString(url, UriKind.Absolute);
        }

        private void CacheUrls(string originalUrl, string shortUrl)
        {
            _cache.Add(originalUrl, shortUrl);
            _cache.Add(shortUrl, originalUrl);
        }
    }
}
