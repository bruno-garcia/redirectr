using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Shortr.Npgsql;

namespace Shortr.Web
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddShortr();
            services.Replace(ServiceDescriptor.Singleton<IShortrStore, NpgsqlShortrStore>());
            services.AddSingleton<NpgsqlShortrOptions>(c => new NpgsqlShortrOptions
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
            app.UseShortr();
        }
    }
}
