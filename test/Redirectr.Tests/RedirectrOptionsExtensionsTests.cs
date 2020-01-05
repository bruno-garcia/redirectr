using System;
using Xunit;

namespace Redirectr.Tests
{
    public class RedirectrOptionsExtensionsTests
    {
        [Fact]
        public void RedirectrOptions_UrlsWithoutLeadingSlash_AreNormalized()
        {
            var target = new RedirectrOptions
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
