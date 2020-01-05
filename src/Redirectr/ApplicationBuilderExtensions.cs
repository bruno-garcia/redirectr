using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Redirectr
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseRedirectr(this IApplicationBuilder builder)
        {
            builder.UseEndpoints(endpoints =>
            {
                var generator = endpoints.ServiceProvider.GetRequiredService<KeyGenerator>();
                var options = endpoints.ServiceProvider.GetRequiredService<IOptions<RedirectrOptions>>().Value;
                // TODO: Validate options such as, options.ShortenUrlPathTemplate is not null or empty
                // BaseAddress exists/valid URL, ShortUrlPathPrefix is at least a slash or a valid path (/a/b/c/)

                var shortenUrlPath = options.ShortenUrlPath;
                if (!options.ShortenUrlPath.EndsWith("/", StringComparison.Ordinal))
                {
                    shortenUrlPath += "/";
                }

                var shortUrlPath = options.ShortUrlPath;
                if (options.ShortUrlPath?.EndsWith("/", StringComparison.Ordinal) != true)
                {
                    shortUrlPath += "/";
                }

                var baseAddress = options.BaseAddress;
                if (!options.BaseAddress.EndsWith("/", StringComparison.Ordinal) && shortUrlPath?[0] != '/')
                {
                    baseAddress += "/";
                }

                // baseAddress now is expected to only be concat to a key to result in a final shortened URL.
                baseAddress += shortUrlPath;

                var whiteListCharactersRegex = new Regex(options.UrlCharacterWhiteList,
                    RegexOptions.Compiled);

                var store = endpoints.ServiceProvider.GetRequiredService<IRedirectrStore>();

                endpoints.MapPut(shortenUrlPath, async context =>
                {
                    if (!context.Request.Query.TryGetValue("url", out var url)
                        || string.IsNullOrWhiteSpace(url)
                        || url[0].Length > options.MaxUrlLength
                        || !whiteListCharactersRegex.IsMatch(url))
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        // TODO: Add body with reason
                        return;
                    }

                    bool useTtl;
                    var intTtl = 0;
                    if ((useTtl = options.AllowDomainTtl
                                  && context.Request.Query.TryGetValue("ttl", out var ttl)
                                  && int.TryParse(ttl, out intTtl))
                        || !await store.TryGetKey(url, out var key))
                    {
                        key = generator.Generate();
                        await store.RegisterUrl(new RegistrationOptions(key, url, useTtl ? intTtl : (int?)null));
                    }

                    context.Response.Headers.Add("Location", baseAddress + key);
                    context.Response.Headers.Add("Key", key);

                    context.Response.StatusCode = (int)HttpStatusCode.Created;
                    await context.Response.CompleteAsync().ConfigureAwait(false);
                });

                endpoints.MapGet(shortUrlPath + "{key}", async context =>
                {
                    var key = (string)context.GetRouteValue("key");
                    if (await store.TryGetUrl(key, out var url))
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.PermanentRedirect;
                        context.Response.Headers.Add("Location", url);
                        context.Response.Headers.Add("Key", key);
                    }
                    else
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    }

                    await context.Response.CompleteAsync().ConfigureAwait(false);
                });
            });
            return builder;
        }
    }
}
