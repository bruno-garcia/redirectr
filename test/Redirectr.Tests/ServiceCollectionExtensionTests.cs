using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Redirectr.Tests
{
    public class ServiceCollectionExtensionTests
    {
        private class Fixture
        {
            public ServiceCollection Services { get; set; } = new ServiceCollection();

            public Dictionary<string, string?> DefaultConfiguration { get; set; } = new Dictionary<string, string?>();

            public Fixture()
            {
                Services.AddRedirectr();
            }

            public IOptions<RedirectrOptions> GetSut()
            {
                var configuration =
                    new ConfigurationBuilder()
                        .AddInMemoryCollection(DefaultConfiguration)
                        .Build();

                Services.AddSingleton<IConfiguration>(configuration);
                var serviceProvider = Services.BuildServiceProvider();
                return serviceProvider.GetService<IOptions<RedirectrOptions>>();
            }
        }

        private readonly Fixture _fixture = new Fixture();

        [Fact]
        public void RedirectrOptions_NoAdditionalConfiguration_ResolvedFromContainer()
        {
            var sut = _fixture.GetSut();

            var actual = sut.Value;

            Assert.NotNull(actual);
            Assert.NotNull(actual.BaseAddress);
            Assert.Equal("http://localhost", actual.BaseAddress);
            Assert.Equal(2048, actual.MaxUrlLength);
            Assert.Equal("shorten", actual.ShortenUrlPath);
            Assert.Equal("s", actual.ShortUrlPath);
            Assert.Null(actual.DestinationWhiteListedDomains);
            Assert.Equal(RedirectrOptions.CharactersWhitelistRegexPattern, actual.UrlCharacterWhiteList);
        }

        [Fact]
        public void RedirectrOptions_PickedUpViaConfiguration()
        {
            const string expectedBaseAddress = "https://nugt.net";
            const int expectedMaxUrlLength = 100;
            const string expectedShortenUrlPath = "c";
            const string expectedShortUrlPath = "u";
            var expectedDestinationWhiteListedDomains = new HashSet<string> {"test", "test2"};
            const string expectedUrlCharacterWhiteList = ".*";
            _fixture.DefaultConfiguration[$"Redirectr:{nameof(RedirectrOptions.BaseAddress)}"] = expectedBaseAddress;
            _fixture.DefaultConfiguration[$"Redirectr:{nameof(RedirectrOptions.MaxUrlLength)}"] = expectedMaxUrlLength.ToString();
            _fixture.DefaultConfiguration[$"Redirectr:{nameof(RedirectrOptions.ShortenUrlPath)}"] = expectedShortenUrlPath;
            _fixture.DefaultConfiguration[$"Redirectr:{nameof(RedirectrOptions.ShortUrlPath)}"] = expectedShortUrlPath;
            _fixture.DefaultConfiguration[$"Redirectr:{nameof(RedirectrOptions.DestinationWhiteListedDomains)}:0"] = "test";
            _fixture.DefaultConfiguration[$"Redirectr:{nameof(RedirectrOptions.DestinationWhiteListedDomains)}:2"] = "test2";
            _fixture.DefaultConfiguration[$"Redirectr:{nameof(RedirectrOptions.UrlCharacterWhiteList)}"] = expectedUrlCharacterWhiteList;

            var sut = _fixture.GetSut();
            var actual = sut.Value;

            Assert.Equal(expectedBaseAddress, actual.BaseAddress);
            Assert.Equal(expectedMaxUrlLength, actual.MaxUrlLength);
            Assert.Equal(expectedShortenUrlPath, actual.ShortenUrlPath);
            Assert.Equal(expectedShortUrlPath, actual.ShortUrlPath);
            Assert.True(expectedDestinationWhiteListedDomains.SequenceEqual(actual.DestinationWhiteListedDomains));
            Assert.Equal(expectedUrlCharacterWhiteList, actual.UrlCharacterWhiteList);
        }

        [Fact]
        public void BaseAddress_ConfigureMadeOptionsInvalid_ValidationsTrigger()
        {
            _fixture.Services.PostConfigure<RedirectrOptions>(o =>
            {
                o.BaseAddress = null!;
                o.MaxUrlLength = -1;
                o.UrlCharacterWhiteList = null!;
                o.ShortenUrlPath = null!;
            });
            var sut = _fixture.GetSut();

            var ex = Assert.Throws<OptionsValidationException>(() => sut.Value);

            Assert.Contains(
                "DataAnnotation validation failed for members: 'BaseAddress' with the error: 'The BaseAddress is required to build the short URLs.'.",
                ex.Failures);

            Assert.Contains(
                "DataAnnotation validation failed for members: 'BaseAddress' with the error: 'The BaseAddress is required to build the short URLs.'.",
                ex.Failures);
        }
    }
}
