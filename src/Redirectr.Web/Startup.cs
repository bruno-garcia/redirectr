using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Redirectr.Npgsql;

namespace Redirectr.Web
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRedirectr();
            services.Replace(ServiceDescriptor.Singleton<IRedirectrStore, NpgsqlRedirectrStore>());
            services.AddSingleton<NpgsqlRedirectrOptions>(c => new NpgsqlRedirectrOptions
            {
                ConnectionString = c.GetRequiredService<IConfiguration>().GetConnectionString("postgres"),
                // Creates the table if needed. Default is true
                CreateSchema = true
            });
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
