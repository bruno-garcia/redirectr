using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using NSubstitute;
using Xunit;

namespace Redirectr.Tests
{
    public class KeyGeneratorTests
    {
        [Fact]
        public void Ctor_NegativeKeyLength_ThrowsException()
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new KeyGenerator(-1));
            Assert.StartsWith("Key length must be at least 1.", ex.Message);
            Assert.Equal("keyLength", ex.ParamName);
        }

        [Fact]
        public void Dispose_DisposesInnerRng()
        {
            var rng = Substitute.For<RandomNumberGenerator>();
            using var target = new KeyGenerator(randomNumberGenerator: rng);
            target.Dispose();
            rng.Received(1).Dispose();
        }

        [Fact]
        public void Generate_DefaultCtor_7CharactersLongKey()
        {
            using var target = new KeyGenerator();
            Assert.Equal(7, target.Generate().Length);
        }

        [Fact]
        public void Generate_KeyLength_DefinesStringLength()
        {
            for (var i = 1; i <= 100; i++)
            {
                using var target = new KeyGenerator(i);
                Assert.Equal(i, target.Generate().Length);
            }
        }

        [Fact]
        public void Generate_MultipleCalls_DontReturnSameResult()
        {
            using var target = new KeyGenerator();

            const int expectedCount = 100;
            var set = new HashSet<string>();

            for (var i = 0; i < expectedCount; i++)
            {
                set.Add(target.Generate());
            }

            Assert.Equal(expectedCount, set.Count);
        }
    }
}
