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

#nullable enable // It's enabled in Directory.Build.props but Rider is buggy
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
            Assert.Equal("http://localhost/", actual.BaseAddress!.AbsoluteUri);
            Assert.Equal(2048, actual.MaxUrlLength);
            Assert.Equal("shorten/", actual.ShortenUrlPath.ToString());
            Assert.Equal("s/", actual.ShortUrlPath!.ToString());
            Assert.Null(actual.DestinationWhiteListedDomains);
            Assert.Equal(RedirectrOptions.CharactersWhitelistRegexPattern, actual.RegexUrlCharacterWhiteList);
        }

        [Fact]
        public void RedirectrOptions_PickedUpViaConfiguration()
        {
            const string expectedBaseAddress = "https://nugt.net/";
            const int expectedMaxUrlLength = 100;
            const string expectedShortenUrlPath = "c/";
            const string expectedShortUrlPath = "u/";
            var expectedDestinationWhiteListedDomains = new HashSet<string> {"test", "test2"};
            const string expectedUrlCharacterWhiteList = ".*";
            _fixture.DefaultConfiguration[$"Redirectr:{nameof(RedirectrOptions.BaseAddress)}"] = expectedBaseAddress;
            _fixture.DefaultConfiguration[$"Redirectr:{nameof(RedirectrOptions.MaxUrlLength)}"] = expectedMaxUrlLength.ToString();
            _fixture.DefaultConfiguration[$"Redirectr:{nameof(RedirectrOptions.ShortenUrlPath)}"] = expectedShortenUrlPath;
            _fixture.DefaultConfiguration[$"Redirectr:{nameof(RedirectrOptions.ShortUrlPath)}"] = expectedShortUrlPath;
            _fixture.DefaultConfiguration[$"Redirectr:{nameof(RedirectrOptions.DestinationWhiteListedDomains)}:0"] = "test";
            _fixture.DefaultConfiguration[$"Redirectr:{nameof(RedirectrOptions.DestinationWhiteListedDomains)}:2"] = "test2";
            _fixture.DefaultConfiguration[$"Redirectr:{nameof(RedirectrOptions.RegexUrlCharacterWhiteList)}"] = expectedUrlCharacterWhiteList;

            var sut = _fixture.GetSut();
            var actual = sut.Value;

            Assert.Equal(expectedBaseAddress, actual.BaseAddress.AbsoluteUri);
            Assert.Equal(expectedMaxUrlLength, actual.MaxUrlLength);
            Assert.Equal(expectedShortenUrlPath, actual.ShortenUrlPath.ToString());
            Assert.Equal(expectedShortUrlPath, actual.ShortUrlPath!.ToString());
            Assert.True(expectedDestinationWhiteListedDomains.SequenceEqual(actual.DestinationWhiteListedDomains));
            Assert.Equal(expectedUrlCharacterWhiteList, actual.RegexUrlCharacterWhiteList);
        }

        [Fact]
        public void BaseAddress_InvalidRegex_ShowsUpInValidation()
        {
            _fixture.Services.PostConfigure<RedirectrOptions>(o => o.RegexUrlCharacterWhiteList = "[");
            var sut = _fixture.GetSut();

            var ex = Assert.Throws<OptionsValidationException>(() => sut.Value);

            Assert.Contains(
                "Custom validation failed for members: 'RegexUrlCharacterWhiteList' with the error: 'Not a valid regular expression pattern.'.",
                ex.Failures);
        }

        [Fact]
        public void BaseAddress_ConfigureMadeOptionsInvalid_ValidationsTrigger()
        {
            _fixture.Services.PostConfigure<RedirectrOptions>(o =>
            {
                o.BaseAddress = null!;
                o.MaxUrlLength = -1;
                o.RegexUrlCharacterWhiteList = null!;
                o.ShortenUrlPath = null!;
            });
            var sut = _fixture.GetSut();

            var ex = Assert.Throws<OptionsValidationException>(() => sut.Value);


            Assert.Contains(
                "DataAnnotation validation failed for members: 'RegexUrlCharacterWhiteList' with the error: 'White list of characters regex is required to validate URLs to be shortened.'.",
                ex.Failures);

            Assert.Contains(
                "DataAnnotation validation failed for members: 'BaseAddress' with the error: 'The BaseAddress is required to build the short URLs.'.",
                ex.Failures);

            Assert.Contains(
                "DataAnnotation validation failed for members: 'ShortenUrlPath' with the error: 'A ShortenUrlPath is required to receive requests to shorten URLs.'.",
                ex.Failures);

            Assert.Contains(
                "DataAnnotation validation failed for members: 'MaxUrlLength' with the error: 'Out of range.'.",
                ex.Failures);
        }

        [Fact]
        public void RedirectrOptions_UrlsWithoutLeadingSlash_AreNormalized()
        {
            const string expectedBaseAddress = "https://nugt.net";
            const string expectedShortenUrlPath = "c";
            const string expectedShortUrlPath = "u";
            _fixture.DefaultConfiguration[$"Redirectr:{nameof(RedirectrOptions.BaseAddress)}"] = expectedBaseAddress;
            _fixture.DefaultConfiguration[$"Redirectr:{nameof(RedirectrOptions.ShortenUrlPath)}"] = expectedShortenUrlPath;
            _fixture.DefaultConfiguration[$"Redirectr:{nameof(RedirectrOptions.ShortUrlPath)}"] = expectedShortUrlPath;

            var sut = _fixture.GetSut();
            var actual = sut.Value;

            Assert.Equal(expectedBaseAddress + "/", actual.BaseAddress.AbsoluteUri);
            Assert.Equal(expectedShortenUrlPath + "/", actual.ShortenUrlPath.ToString());
            Assert.Equal(expectedShortUrlPath + "/", actual.ShortUrlPath!.ToString());
        }
    }
}
