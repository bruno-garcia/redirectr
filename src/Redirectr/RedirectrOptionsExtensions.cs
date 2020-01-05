using System;

namespace Redirectr
{
    public static class RedirectrOptionsExtensions
    {
        public static void Normalize(this RedirectrOptions options)
        {
            // Normalize URLs. Blindly appending a forward slash to a Uri instance results in double slash
            // when that instance already terminated with a slash. To simplify the code further,
            // Options is expected to include a leading slash to all its URLs:
            var shortenUrlPath = options.ShortenUrlPath.ToString();
            if (!shortenUrlPath.EndsWith("/", StringComparison.Ordinal))
            {
                shortenUrlPath += "/";
                options.ShortenUrlPath = new Uri(shortenUrlPath, UriKind.Relative);
            }

            var shortUrlPath = options.ShortUrlPath?.ToString();
            if (shortUrlPath?.EndsWith("/", StringComparison.Ordinal) != true)
            {
                shortUrlPath += "/";
                options.ShortUrlPath = new Uri(shortUrlPath, UriKind.Relative);
            }

            // baseAddress now is expected to only be concat to a key to result in a final shortened URL.
            var baseAddress = options.BaseAddress?.AbsoluteUri;
            if (baseAddress is {} && !baseAddress.EndsWith("/", StringComparison.Ordinal) && shortUrlPath?[0] != '/')
            {
                baseAddress += "/";
                options.BaseAddress = new Uri(baseAddress, UriKind.Absolute);
            }
        }
    }
}
