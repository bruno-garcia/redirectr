using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Shortr.Web.Tests
{
    public class IntegrationTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        public class Fixture
        {
            private readonly WebApplicationFactory<Startup> _factory;
            public Action<ShortrOptions>? ShortrOptionsConfiguration { get; set; }

            public WebApplicationFactoryClientOptions WebApplicationFactoryClientOptions { get; set; } =
                new WebApplicationFactoryClientOptions {AllowAutoRedirect = false};

            public Action<IServiceCollection>? ConfigureServices { get; set; }

            public IServiceProvider Provider => _factory.Services;

            public Fixture(WebApplicationFactory<Startup> factory) => _factory = factory;

            public HttpClient GetClient() =>
                _factory.WithWebHostBuilder(b =>
                    b.ConfigureServices(s =>
                    {
                        s.Replace(ServiceDescriptor.Singleton<IShortrStore, InMemoryShortrStore>());
                        s.Configure<ShortrOptions>(o => ShortrOptionsConfiguration?.Invoke(o));
                        ConfigureServices?.Invoke(s);
                    })).CreateClient(WebApplicationFactoryClientOptions);
        }

        private readonly Fixture _fixture;
        public IntegrationTests(WebApplicationFactory<Startup> factory) => _fixture = new Fixture(factory);

        [Fact]
        public async Task RoundTrip()
        {
            _fixture.ShortrOptionsConfiguration = o => o.BaseAddress = new Uri("https://nugt.net");
            var options = _fixture.Provider.GetRequiredService<IOptions<ShortrOptions>>().Value;
            var client = _fixture.GetClient();

            const string longUrl = "https://nugt.net/packages?months=12&ids=Sentry";
            var shorten = $"/{options.ShortenUrlPath}?url={HttpUtility.UrlEncode(longUrl)}";

            var response = await client.PutAsync(shorten, null);

            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.True(response.Headers.TryGetValues("Key", out var keys));
            Assert.True(response.Headers.TryGetValues("Location", out var locations));
            var key = Assert.Single(keys);
            var location = Assert.Single(locations);
            Assert.EndsWith(key, location);

            response = await client.GetAsync(location);
            Assert.Equal(HttpStatusCode.PermanentRedirect, response.StatusCode);
            Assert.Equal(longUrl, response.Headers.Location.AbsoluteUri);
        }

        [Fact]
        public async Task Put_TargetUrlPartOfBaseAddressWithShortPath_BadRequest()
        {
            _fixture.ShortrOptionsConfiguration = o => o.BaseAddress = new Uri("http://nugt.net");
            var options = _fixture.Provider.GetRequiredService<IOptions<ShortrOptions>>().Value;
            var client = _fixture.GetClient();

            const string longUrl = "http://nugt.net/s/some-short-url";
            var shorten = $"/{options.ShortenUrlPath}?url={HttpUtility.UrlEncode(longUrl)}";

            var response = await client.PutAsync(shorten, null);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Put_TargetUrlPartOfBaseAddressWithShortenPath_BadRequest()
        {
            _fixture.ShortrOptionsConfiguration = o => o.BaseAddress = new Uri("http://nugt.net");
            var options = _fixture.Provider.GetRequiredService<IOptions<ShortrOptions>>().Value;
            var client = _fixture.GetClient();

            const string longUrl = "http://nugt.net/shorten/some-short-url";
            var shorten = $"/{options.ShortenUrlPath}?url={HttpUtility.UrlEncode(longUrl)}";

            var response = await client.PutAsync(shorten, null);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Put_SameDomainDefaultOptions_Created()
        {
            _fixture.ShortrOptionsConfiguration = o => o.BaseAddress = new Uri("http://nugt.net");
            var options = _fixture.Provider.GetRequiredService<IOptions<ShortrOptions>>().Value;
            var client = _fixture.GetClient();

            const string longUrl = "http://nugt.net/packages?months=12&ids=Sentry";
            var shorten = $"/{options.ShortenUrlPath}?url={HttpUtility.UrlEncode(longUrl)}";

            var response = await client.PutAsync(shorten, null);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task Put_DifferentDomainDefaultOptions_BadRequest()
        {
            _fixture.ShortrOptionsConfiguration = o => o.BaseAddress = new Uri("http://nugt.net");
            var options = _fixture.Provider.GetRequiredService<IOptions<ShortrOptions>>().Value;
            var client = _fixture.GetClient();

            const string longUrl = "http://nugettrends.com/packages?months=12&ids=Sentry";
            var shorten = $"/{options.ShortenUrlPath}?url={HttpUtility.UrlEncode(longUrl)}";

            var response = await client.PutAsync(shorten, null);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Put_AllowRedirectToAnyDomainTrue_TargetUrlPartOfBaseAddress_Created()
        {
            _fixture.ShortrOptionsConfiguration = o =>
            {
                o.AllowRedirectToAnyDomain = true;
                o.BaseAddress = new Uri("http://nugt.net");
            };
            var options = _fixture.Provider.GetRequiredService<IOptions<ShortrOptions>>().Value;
            var client = _fixture.GetClient();

            const string longUrl = "http://nugt.net/packages?months=12&ids=Sentry";
            var shorten = $"/{options.ShortenUrlPath}?url={HttpUtility.UrlEncode(longUrl)}";

            var response = await client.PutAsync(shorten, null);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task Put_WhiteListDestination_Success()
        {
            _fixture.ShortrOptionsConfiguration =
                o => o.DestinationWhiteListedBaseAddresses = new[] {new Uri("http://nugt.net")};

            var options = _fixture.Provider.GetRequiredService<IOptions<ShortrOptions>>().Value;
            var client = _fixture.GetClient();

            const string longUrl = "http://nugt.net/packages?months=12&ids=Sentry";
            var shorten = $"/{options.ShortenUrlPath}?url={HttpUtility.UrlEncode(longUrl)}";

            var response = await client.PutAsync(shorten, null);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task Put_NotWhiteListDestination_BadRequest()
        {
            _fixture.ShortrOptionsConfiguration =
                o => o.DestinationWhiteListedBaseAddresses = new[] {new Uri("http://nugt.net")};

            var options = _fixture.Provider.GetRequiredService<IOptions<ShortrOptions>>().Value;
            var client = _fixture.GetClient();

            const string longUrl = "http://nugettrends.com/packages?months=12&ids=Sentry";
            var shorten = $"/{options.ShortenUrlPath}?url={HttpUtility.UrlEncode(longUrl)}";

            var response = await client.PutAsync(shorten, null);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
