using System;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Redirectr
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRedirectr(this IServiceCollection services)
        {
            services.TryAddSingleton<KeyGenerator>();
            services.TryAddSingleton<IRedirectrStore, InMemoryRedirectrStore>();

            services
                .AddOptions<RedirectrOptions>()
                .Configure<IConfiguration>((o, c) => c.Bind("Redirectr", o))
                .PostConfigure((RedirectrOptions o, IConfiguration c) =>
                {
                    if (o.BaseAddress is null)
                    {
                        o.BaseAddress = new Uri(
                            c.GetValue<string>("URLS")?
                                .Split(";")?
                                .FirstOrDefault()
                            ?? "http://localhost",
                            UriKind.Absolute); // With TestServer 'URLS' isn't defined
                    }

                    // Normalize URLs. Blindly appending a forward slash to a Uri instance results in double slash
                    // when that instance already terminated with a slash. To simplify the code further,
                    // Options is expected to include a leading slash to all its URLs:
                    var shortenUrlPath = o.ShortenUrlPath.ToString();
                    if (!shortenUrlPath.EndsWith("/", StringComparison.Ordinal))
                    {
                        shortenUrlPath += "/";
                        o.ShortenUrlPath = new Uri(shortenUrlPath, UriKind.Relative);
                    }

                    var shortUrlPath = o.ShortUrlPath?.ToString();
                    if (shortUrlPath?.EndsWith("/", StringComparison.Ordinal) != true)
                    {
                        shortUrlPath += "/";
                        o.ShortUrlPath = new Uri(shortUrlPath, UriKind.Relative);
                    }

                    // baseAddress now is expected to only be concat to a key to result in a final shortened URL.
                    var baseAddress = o.BaseAddress.AbsoluteUri;
                    if (!baseAddress.EndsWith("/", StringComparison.Ordinal) && shortUrlPath?[0] != '/')
                    {
                        baseAddress += "/";
                        o.BaseAddress = new Uri(baseAddress, UriKind.Absolute);
                    }

                })
                .Validate(o =>
                {
                    try
                    {
                        _ = Regex.IsMatch(string.Empty, o.RegexUrlCharacterWhiteList);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }, "Custom validation failed for members: 'RegexUrlCharacterWhiteList' with the error: 'Not a valid regular expression pattern.'.")
                .ValidateDataAnnotations();

            return services;
        }
    }
}
