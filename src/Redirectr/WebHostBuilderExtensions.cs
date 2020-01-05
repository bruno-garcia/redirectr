using System;
using System.ComponentModel;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Configuration;

namespace Redirectr
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class WebHostBuilderExtensions
    {
        public static IWebHostBuilder UseRedirectr(
            this IWebHostBuilder builder,
            Action<RedirectrOptions>? configureOptions = null)
        {
            builder.ConfigureLogging((context, logging) =>
            {
                logging.AddConfiguration();

                var section = context.Configuration.GetSection("Redirectr");
                logging.Services.Configure<RedirectrOptions>(section);
                logging.Services.Configure<RedirectrOptions>(c =>
                {
                    if (c.BaseAddress is null)
                    {
                        var baseAddress = context.Configuration?.GetValue<string>("URLS")?
                            .Split(";")?
                            .FirstOrDefault()
                            // With TestServer 'URLS' isn't defined
                            ?? "http://localhost";

                        c.BaseAddress = baseAddress;
                    }
                });

                if (configureOptions != null)
                {
                    logging.Services.Configure(configureOptions);
                }
            });

            builder.ConfigureServices(c => c.AddRedirectr());
            builder.Configure(c =>
            {
                c.UseRouting(); // TODO: This is likely a bad idea. User might need to control when this happens
                c.UseRedirectr();
            });

            return builder;
        }
    }
}
