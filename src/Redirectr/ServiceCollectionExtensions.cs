using System.ComponentModel;
using System.Linq;
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
                    if (string.IsNullOrWhiteSpace(o.BaseAddress))
                    {
                        o.BaseAddress = c.GetValue<string>("URLS")?
                                          .Split(";")?
                                          .FirstOrDefault()
                                      ?? "http://localhost"; // With TestServer 'URLS' isn't defined
                    }
                })
                .ValidateDataAnnotations();

            return services;
        }
    }
}
