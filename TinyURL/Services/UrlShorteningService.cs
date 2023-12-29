using TinyURL.Core.Interfaces;
using TinyURL.Core.Models;

namespace TinyURL.Services
{
    public class UrlShorteningService : IUrlShorteningService
    {
        private readonly IUrlEntryRepository _urlEntryRepository;
        private readonly IBase62Encoding _base62;
        private readonly ICustomMemoryCache<string> _cache;
        private readonly UrlShorteningSettings _settings;


        public UrlShorteningService(UrlShorteningSettings settings,
                                    IUrlEntryRepository urlEntryRepository,
                                    ICustomMemoryCache<string> cache,
                                    IBase62Encoding base62)
        {
            _settings = settings;
            _urlEntryRepository = urlEntryRepository;
            _cache = cache;
            _base62 = base62;
        }




        private bool IsValidUrl(string url)
        {
            // You can extend this method to include more sophisticated checks if needed
            return Uri.IsWellFormedUriString(url, UriKind.Absolute);
        }

        /// <summary>
        /// Shortens a given URL by either retrieving a cached version or storing a new one in the database.
        /// </summary>
        /// <param name="originalUrl">The original URL to be shortened.</param>
        /// <returns>A task that represents the asynchronous operation, returning the urlMapping object for the shortened URL.</returns>

        public async Task<urlMapping> ShortenUrlAsync(string originalUrl)
        {
            if (string.IsNullOrWhiteSpace(originalUrl) || !IsValidUrl(originalUrl))
            {
                throw new ArgumentException("Original URL is not valid.", nameof(originalUrl));
            }

            urlMapping urlEntry = null;
            string cachedShortUrl = null;

            // Check if the URL is in cache
            bool isCached = _cache.TryGetValue(originalUrl, out cachedShortUrl);

            if (!isCached)
            {
                // If not in cache, check in the database
                urlEntry = await _urlEntryRepository.GetUrlByOriginalUrlAsync(originalUrl);

                if (urlEntry != null)
                {
                    // Found in database, add to cache
                    _cache.Add(originalUrl, urlEntry.ShortUrl);
                    _cache.Add(urlEntry.ShortUrl, originalUrl);
                }
                else
                {
                    // Not found in the cache and not in the database generate new one
                    string shortUrl = _settings.BaseUrl + await GenerateShortUrl(); // Prefix BaseUrl to the generated short URL
                    urlEntry = new urlMapping
                    {
                        OriginalUrl = originalUrl,
                        ShortUrl = shortUrl
                    };
                    await _urlEntryRepository.AddUrlAsync(urlEntry);
                    _cache.Add(originalUrl, shortUrl);
                    _cache.Add(shortUrl, originalUrl);
                }
            }
            else
            {
                // Found in cache, create a new urlMapping object
                urlEntry = new urlMapping
                {
                    OriginalUrl = originalUrl,
                    ShortUrl = cachedShortUrl
                };
            }

            return urlEntry;
        }


        public async Task<string> ExpandUrlAsync(string shortUrl)
        {
            shortUrl = _settings.BaseUrl + shortUrl;
            if (string.IsNullOrWhiteSpace(shortUrl))
            {
                throw new ArgumentException("Short URL cannot be null or whitespace.", nameof(shortUrl));
            }

            string originalUrl = null;
            string cachedOriginalUrl = null;

            // Check if the short URL is in cache
            bool isCached = _cache.TryGetValue(shortUrl, out cachedOriginalUrl);

            if (!isCached)
            {
                // If not in cache, check in the database
                var urlEntry = await _urlEntryRepository.GetUrlByShortUrlAsync(shortUrl);

                if (urlEntry != null)
                {
                    // Found in database, add to cache
                    _cache.Add(shortUrl, urlEntry.OriginalUrl);
                    _cache.Add(urlEntry.OriginalUrl, shortUrl);
                    originalUrl = urlEntry.OriginalUrl;
                }
                else
                {
                    // Not found in the cache and not in the database
                    throw new InvalidOperationException("Short URL does not correspond to an existing original URL.");
                }
            }
            else
            {
                // Found in cache
                originalUrl = cachedOriginalUrl;
            }

            return originalUrl;
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
                // Log the exception details here
                // You can also decide to throw a custom exception or handle it based on your application's needs
                throw new InvalidOperationException("An error occurred while generating the short URL.", ex);
            }
        }



    }
}
