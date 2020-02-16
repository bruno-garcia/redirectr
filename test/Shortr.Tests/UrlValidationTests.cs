using System;
using System.Linq;
using Xunit;

namespace Shortr.Tests
{
    public class UrlValidationTests
    {
        public class Fixture
        {
            public ShortrOptions Options { get; set; } = new ShortrOptions
            {
                // A base address is required
                BaseAddress = new Uri("https://nugettrends.com")
            };

            public UrlValidation GetSut() => new UrlValidation(Options);
        }

        private readonly Fixture _fixture = new Fixture();

        [Theory]
        [InlineData(false, (string?)null, false)]
        [InlineData(false, "", false)]
        [InlineData(false, "[", false)]
        [InlineData(false, "/relative", false)]
        [InlineData(false, @"\\relative", false)]
        [InlineData(false, "http://localhost/\r", false)]
        [InlineData(false, "http://localhost/ø", false)]
        [InlineData(false, "http://localho\0st", false)]
        [InlineData(false, "file://localhost", false)]
        [InlineData(false, "smb://localhost", false)]
        [InlineData(true, "http://localhost", true)]
        [InlineData(true, "https://localhost", true)]
        [InlineData(true, "http://localhost:1234", true)]
        [InlineData(true, "http://localhost/", true)]
        [InlineData(true, "https://localhost/", true)]
        public void IsValidUrl_AllowRedirectToAnyDomain_TestCases(bool allowRedirectToAnyDomain, string? url, bool isValid)
        {
            _fixture.Options.AllowRedirectToAnyDomain = allowRedirectToAnyDomain;
            Assert.Equal(isValid, _fixture.GetSut().IsValidUrl(url!));
        }

        [Fact]
        public void Ctor_NullBaseAddress_InvalidArgumentException()
        {
            _fixture.Options.BaseAddress = null!;
            Assert.Throws<ArgumentException>(() => _fixture.GetSut());
        }

        [Fact]
        public void IsValidUrl_LargerThanDefaultLength_IsNotValid()
        {
            const string address = "http://domain.io/q=";
            var queryLength = ShortrOptions.DefaultMaxUrlLength - address.Length + 1;
            var url = $"{address}{new string('a', queryLength)}";
            var sut = _fixture.GetSut();
            Assert.False(sut.IsValidUrl(url));
        }

        [Fact]
        public void IsValidUrl_SameAsDefaultLengthWithDefault_IsNotValid()
        {
            const string address = "http://domain.io/q=";
            var queryLength = ShortrOptions.DefaultMaxUrlLength - address.Length;
            var url = $"{address}{new string('a', queryLength)}";
            var sut = _fixture.GetSut();
            Assert.False(sut.IsValidUrl(url));
        }

        [Fact]
        public void IsValidUrl_SameAsDefaultLength_IsValid()
        {
            _fixture.Options.AllowRedirectToAnyDomain = true;
            const string address = "http://domain.io/q=";
            var queryLength = ShortrOptions.DefaultMaxUrlLength - address.Length;
            var url = $"{address}{new string('a', queryLength)}";
            var sut = _fixture.GetSut();
            Assert.True(sut.IsValidUrl(url));
        }

        [Fact]
        public void IsValidUrl_SameAsBaseAddress_IsValid()
        {
            _fixture.Options.AllowRedirectToAnyDomain = true;
            // Matches other locations in the same URL, except the short/shorten
            var url = new Uri("http://localhost:1324");
            _fixture.Options.BaseAddress = url;
            var sut = _fixture.GetSut();
            Assert.True(sut.IsValidUrl(url.AbsoluteUri));
        }

        [Fact]
        public void IsValidUrl_SameAsBaseAddressWithPathWithAllowRedirectToAnyDomain_IsValid()
        {
            _fixture.Options.AllowRedirectToAnyDomain = true;
            var url = new Uri("http://localhost:1324");
            _fixture.Options.BaseAddress = url;
            var sut = _fixture.GetSut();
            Assert.True(sut.IsValidUrl(new Uri(url, "something/else/").AbsoluteUri));
        }

        [Fact]
        public void IsValidUrl_SameAsBaseAddressWithPathDefault_IsValid()
        {
            var url = new Uri("http://localhost:1324");
            _fixture.Options.BaseAddress = url;
            var sut = _fixture.GetSut();
            Assert.True(sut.IsValidUrl(new Uri(url, "something/else/").AbsoluteUri));
        }

        [Fact]
        public void IsValidUrl_SameAsBaseAddressWithPathSameAsShorten_IsNotValid()
        {
            var url = new Uri("http://localhost:1324");
            _fixture.Options.BaseAddress = url;
            var sut = _fixture.GetSut();
            Assert.False(sut.IsValidUrl(new Uri(
                url,
                _fixture.Options.ShortenUrlPath).AbsoluteUri));
        }

        [Fact]
        public void IsValidUrl_SameAsBaseAddressWithPathSameAsShort_IsNotValid()
        {
            var url = new Uri("http://localhost:1324");
            _fixture.Options.BaseAddress = url;
            var sut = _fixture.GetSut();
            Assert.False(sut.IsValidUrl(new Uri(
                url,
                _fixture.Options.ShortUrlPath!).AbsoluteUri));
        }

        [Fact]
        public void IsValidUrl_DifferentThanBaseAddressWithDefaultValues_IsNotValid()
        {
            var baseUrl = new Uri($"http://localhost:1324");
            var url = new Uri($"http://localhost:1325");
            _fixture.Options.BaseAddress = baseUrl;
            var sut = _fixture.GetSut();
            Assert.False(sut.IsValidUrl(url.AbsoluteUri));
        }

        [Fact]
        public void IsValidUrl_DifferentThanBaseAddressWithAllowRedirectToAnyDomainTrue_IsValid()
        {
            _fixture.Options.AllowRedirectToAnyDomain = true;
            var baseUrl = new Uri($"http://localhost:1324");
            var url = new Uri($"http://localhost:1325");
            _fixture.Options.BaseAddress = baseUrl;
            var sut = _fixture.GetSut();
            Assert.True(sut.IsValidUrl(url.AbsoluteUri));
        }

        [Theory]
        [InlineData(true, "http://localhost/", "http://a", "http://a/")]
        [InlineData(true, "http://localhost/", "http://a.io/something/#else", "http://a.io", "http://localhost:5000")]
        [InlineData(true, "http://localhost/", "http://a/b/c", "http://a", "http://b")]
        [InlineData(true, "http://localhost/", "http://a/b/c", "http://a")]
        [InlineData(true, "http://a", "http://a", "http://a")]
        [InlineData(false, "http://a", "http://a/shorten/?url=http://a", "http://a")]
        [InlineData(false, "http://a", "http://a/s/?url=http://a", "http://a")]
        [InlineData(false, "http://a", "http://b", "http://c")]
        [InlineData(false, "https://a", "https://b", "https://c")]
        [InlineData(false, "https://a", "http://b/c?d=e", "https://c")]
        public void IsValidUrl_BaseAddressAndDestinationWhiteListedBaseAddresses_TestCases(
            bool isValid, string baseAddress, string url, params string[] whiteList)
        {
            _fixture.Options.BaseAddress = new Uri(baseAddress);
            _fixture.Options.DestinationWhiteListedBaseAddresses = whiteList.Select(l => new Uri(l));
            var sut = _fixture.GetSut();

            Assert.Equal(isValid, sut.IsValidUrl(url));
        }
    }
}
