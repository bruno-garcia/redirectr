using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Redirectr.Web
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration) => _configuration = configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            var section = _configuration.GetSection("Redirectr");
            services.Configure<RedirectrOptions>(section);
            services.Configure<RedirectrOptions>(c =>
            {
                if (c.BaseAddress is null)
                {
                    var baseAddress = _configuration?.GetValue<string>("URLS")?
                                          .Split(";")?
                                          .FirstOrDefault()
                                      // With TestServer 'URLS' isn't defined
                                      ?? "http://localhost";

                    c.BaseAddress = baseAddress;
                }
            });

            services.AddRedirectr();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseRedirectr();
        }
    }
}
