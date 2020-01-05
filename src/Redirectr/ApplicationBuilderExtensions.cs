using System;
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
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseRedirectr(this IApplicationBuilder builder)
        {
            builder.UseEndpoints(endpoints =>
            {
                var generator = endpoints.ServiceProvider.GetRequiredService<KeyGenerator>();
                var options = endpoints.ServiceProvider.GetRequiredService<IOptions<RedirectrOptions>>().Value;

                var shortenUrlPath = options.ShortenUrlPath.ToString();
                var shortUrlPath = options.ShortUrlPath;

                var baseAddress = options.BaseAddress;

                // baseAddress now is expected to only be concat to a key to result in a final shortened URL.
                var baseAddressShortUrl = new Uri(baseAddress, shortUrlPath!);

                var whiteListCharactersRegex = new Regex(options.RegexUrlCharacterWhiteList,
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

                    context.Response.Headers.Add("Location", baseAddressShortUrl + key);
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
