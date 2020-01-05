using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Redirectr
{
    /// <summary>
    /// Options to configure Redirectr
    /// </summary>
    public class RedirectrOptions
    {
        // NOTE: This is very naïve. A URL such as https://duckduckgo.com/?q=naïve is totally valid.
        private const string CharactersWhitelistRegexPattern =
            @"^[ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789\-._~:\/?#[\]\@\!\$\&\'\(\)\*\+\,\;\=]+$";

        [Required(ErrorMessage = "White list of characters regex is required to validate URLs to be shortened.")]
        public string UrlCharacterWhiteList { get; set; } = CharactersWhitelistRegexPattern;

        /// <summary>
        /// BaseAddress for shortened URLs.
        /// </summary>
        [Required(ErrorMessage = "The BaseAddress is required to build the short URLs.")]
        public string BaseAddress { get; set; } = default!; // https://nugt.net

        /// <summary>
        /// The URL path to shorten links via a PUT request.
        /// </summary>
        /// <example>
        /// Default value would mean baseAddress + shorten:
        /// http://localhost/shorten?url=https://nugt.net/example
        /// </example>
        [Required(ErrorMessage = "A ShortenUrlPath is required to receive requests to shorten URLs.")]
        public string ShortenUrlPath { get; set; } = "shorten";

        /// <summary>
        /// The path for a short URL.
        /// </summary>
        /// <example>
        /// null or an empty string would mean the root i.e: GET /h6d0P
        /// Value 's' would mean short URLs are under /s/ i.e: GET /a/h6d0P
        /// </example>
        public string? ShortUrlPath { get; set; } = "s";

        /// <summary>
        /// Used to compare against destination URLs
        /// </summary>
        /// <example>
        /// Having https://nugettrends.com in this list means a URL shortening request for
        /// https://nugettrends.com/packages?months=12&amp;ids=Sentry&amp;ids=Sentry.Protocol
        /// would be accepted, but for https://www.nugettrends... would not.
        /// </example>
        public HashSet<string>? DestinationWhiteListedDomains { get; set; }

        /// <summary>
        /// The max length of the URL to shorten.
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Out of range.")]
        public int MaxUrlLength { get; set; } = 2048;

        /// <summary>
        /// Whether shorten URL request can specify a TTL.
        /// </summary>
        /// <remarks>
        /// This allows the creation of links that expire.
        /// </remarks>
        public bool AllowDomainTtl { get; set; } = false;
    }
}
