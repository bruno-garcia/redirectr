using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Redirectr
{
    internal class RedirectrOptionsSetup : IConfigureOptions<RedirectrOptions>
    {
        private readonly IConfiguration _configuration;

        public RedirectrOptionsSetup(IConfiguration configuration) => _configuration = configuration;

        public void Configure(RedirectrOptions options)
        {
            _configuration.GetSection("Redirectr").Bind(options);
            if (options.BaseAddress is null)
            {
                options.BaseAddress = _configuration?
                                          .GetValue<string>("URLS")?
                                          .Split(";")?
                                          .FirstOrDefault()
                                      ?? "http://localhost"; // With TestServer 'URLS' isn't defined
            }
        }
    }
}
