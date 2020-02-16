using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Shortr
{
    public class UrlValidation
    {
        private readonly ShortrOptions _options;
        private readonly Regex _whiteListCharactersRegex;
        private readonly Uri _baseShortenUrl;
        private readonly Uri _baseShortUrl;

        public UrlValidation(ShortrOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));

            // BaseAddress should not be null (non nullable/servive library throws on start).
            if (_options.BaseAddress is null)
            {
                throw new ArgumentException("The service BaseAddress is required.", nameof(options));
            }

            _whiteListCharactersRegex = new Regex(options.RegexUrlCharacterWhiteList, RegexOptions.Compiled);
            _baseShortenUrl = new Uri(_options.BaseAddress, _options.ShortenUrlPath);
            _baseShortUrl = new Uri(_options.BaseAddress, _options.ShortUrlPath);
        }

        public bool IsValidUrl(string targetUrl)
        {
            if (string.IsNullOrWhiteSpace(targetUrl)
                || targetUrl.Length > _options.MaxUrlLength
                || !_whiteListCharactersRegex.IsMatch(targetUrl))
            {
                return false;
            }

            try
            {
                // Can construct Uri instance:
                var uri = new Uri(targetUrl, UriKind.Absolute);

                if (!targetUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    // relative path like '/something/ parses successfully as: 'file://something/'.
                    return false;
                }

                // Unless whitelisted, can't create a short link to a shorten URL request link
                if (_baseShortenUrl.IsBaseOf(uri))
                {
                    return false;
                }

                // Unless whitelisted, can't create a short link to a short link
                if (_baseShortUrl.IsBaseOf(uri))
                {
                    return false;
                }

                // If any URL is OK, just return here.
                if (_options.AllowRedirectToAnyDomain)
                {
                    return true;
                }

                // Base address is allowed by default
                if (_options.BaseAddress.IsBaseOf(uri))
                {
                    return true;
                }

                // If it's whitelisted, it's set.
                if (_options.DestinationWhiteListedBaseAddresses is {} whiteList)
                {
                    return whiteList.Any(u => u.IsBaseOf(uri));
                }

                // No rule matched, so it's not valid.
                return false;
            }
            catch
            {
                // Log here, use reason on 400
                return false;
            }
        }
    }
}
