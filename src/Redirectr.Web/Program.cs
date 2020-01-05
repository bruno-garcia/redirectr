using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Redirectr.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    // webBuilder.UseRedirectr();
                    webBuilder.UseStartup<Startup>();
                });
    }
}
