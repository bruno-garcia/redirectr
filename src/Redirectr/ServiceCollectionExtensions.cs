using System.ComponentModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Redirectr
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRedirectr(this IServiceCollection services)
        {
            services.TryAddSingleton<KeyGenerator>();
            services.TryAddSingleton<IRedirectrStore, InMemoryRedirectrStore>();

            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IConfigureOptions<RedirectrOptions>, RedirectrOptionsSetup>());

            services
                .AddOptions<RedirectrOptions>()
                .Configure<IConfiguration>((o, c) => c.Bind("Redirectr", o));

            return services;
        }
    }
}
