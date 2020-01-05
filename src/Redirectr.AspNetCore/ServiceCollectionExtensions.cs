using System;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Redirectr;

// ReSharper disable once CheckNamespace -- Discoverability
namespace Microsoft.Extensions.DependencyInjection
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRedirectr(this IServiceCollection services)
        {
            services.TryAddSingleton<IKeyGenerator, KeyGenerator>();
            services.TryAddSingleton<IRedirectrStore, InMemoryRedirectrStore>();

            services
                .AddOptions<RedirectrOptions>()
                .Configure<IConfiguration>((o, c) => c.Bind("Redirectr", o))
                .PostConfigure((RedirectrOptions o, IConfiguration c) =>
                {
                    o.Normalize();

                    if (o.BaseAddress is null)
                    {
                        o.BaseAddress = new Uri(
                            c.GetValue<string>("URLS")?
                                .Split(";")?
                                .FirstOrDefault()
                            ?? "http://localhost",
                            UriKind.Absolute); // With TestServer 'URLS' isn't defined
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
                },
                "Custom validation failed for members: 'RegexUrlCharacterWhiteList' with the error: 'Not a valid regular expression pattern.'.")
                .ValidateDataAnnotations()
                .Validate(o => o.BaseAddress?.IsAbsoluteUri is true, "Must be an absolute URI.");

            return services;
        }
    }
}
