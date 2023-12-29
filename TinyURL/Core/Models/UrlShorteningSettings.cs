namespace TinyURL.Core.Models
{
    public class UrlShorteningSettings
    {
        public long ?BaseNumber { get; set; }
        public string BaseUrl { get; set; } = "";
    }
}
