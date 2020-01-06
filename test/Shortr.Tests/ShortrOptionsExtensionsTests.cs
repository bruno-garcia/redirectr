using System;
using Xunit;

namespace Shortr.Tests
{
    public class ShortrOptionsExtensionsTests
    {
        [Fact]
        public void ShortrOptions_UrlsWithoutLeadingSlash_AreNormalized()
        {
            var target = new ShortrOptions
            {
                BaseAddress = new Uri("https://nugt.net"),
                ShortenUrlPath = new Uri("c", UriKind.Relative),
                ShortUrlPath = new Uri("u", UriKind.Relative),
            };

            target.Normalize();

            Assert.Equal("https://nugt.net/", target.BaseAddress.AbsoluteUri);
            Assert.Equal("c/", target.ShortenUrlPath.ToString());
            Assert.Equal("u/", target.ShortUrlPath!.ToString());
        }
    }
}
