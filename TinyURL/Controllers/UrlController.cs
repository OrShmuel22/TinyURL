using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TinyURL.Core.Interfaces;

namespace TinyURL.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UrlController : ControllerBase
    {
        private readonly IUrlShorteningService _urlShorteningService;

        public UrlController(IUrlShorteningService urlShorteningService)
        {
            _urlShorteningService = urlShorteningService;
        }

        [HttpPost("shorten")]
        public async Task<ActionResult> ShortenUrl([FromBody] string originalUrl)
        {
            if (string.IsNullOrWhiteSpace(originalUrl))
            {
                return BadRequest("Invalid URL.");
            }

            var shortenedUrl = await _urlShorteningService.ShortenUrlAsync(originalUrl);
            return Ok(shortenedUrl);
        }

        [HttpGet("expand/{shortUrl}")]
        public async Task<ActionResult> ExpandUrl(string shortUrl)
        {
            var originalUrl = await _urlShorteningService.ExpandUrlAsync(shortUrl);

            if (originalUrl == null)
            {
                return NotFound("URL not found.");
            }

            return Ok(originalUrl);
        }

        // New action method for redirecting short URLs to original URLs
        [HttpGet("{code}")]
        public async Task<IActionResult> RedirectShortUrl(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return NotFound("Short URL code is invalid.");
            }

            string originalUrl = await _urlShorteningService.ExpandUrlAsync(code);

            if (originalUrl == null)
            {
                return NotFound("URL not found.");
            }

            return Redirect(originalUrl);
        }
    }
}
