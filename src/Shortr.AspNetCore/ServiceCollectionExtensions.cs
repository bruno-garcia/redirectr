using System;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Shortr;

// ReSharper disable once CheckNamespace -- Discoverability
namespace Microsoft.Extensions.DependencyInjection
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddShortr(this IServiceCollection services)
        {
            services.TryAddSingleton<UrlValidation>();
            services.TryAddSingleton<IKeyGenerator, KeyGenerator>();
            services.TryAddSingleton<IShortrStore, InMemoryShortrStore>();
            services.AddSingleton(c => c.GetService<IOptions<ShortrOptions>>().Value);

            services
                .AddOptions<ShortrOptions>()
                .Configure<IConfiguration>((o, c) => c.Bind("Shortr", o))
                .PostConfigure((ShortrOptions o, IConfiguration c) =>
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
