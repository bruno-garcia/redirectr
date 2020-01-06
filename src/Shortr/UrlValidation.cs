using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Shortr
{
    public class UrlValidation
    {
        private readonly ShortrOptions _options;
        private readonly Regex _whiteListCharactersRegex;

        public UrlValidation(ShortrOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));

            // BaseAddress should not be null (non nullable/servive library throws on start).
            if (_options.BaseAddress is null)
            {
                throw new ArgumentException("The service BaseAddress is required.", nameof(options));
            }

            _whiteListCharactersRegex = new Regex(options.RegexUrlCharacterWhiteList, RegexOptions.Compiled);
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

                // Can't create a short link to a short link
                if (uri.AbsoluteUri.StartsWith(_options.BaseAddress.AbsoluteUri,StringComparison.InvariantCultureIgnoreCase))
                {
                    return false;
                }

                if (_options.DestinationWhiteListedBaseAddresses is {} whiteList)
                {
                    return whiteList.Any(u => u.IsBaseOf(uri));
                }

                // No domain to match
                return true;
            }
            catch
            {
                // Log here, use reason on 400
                return false;
            }
        }
    }
}
