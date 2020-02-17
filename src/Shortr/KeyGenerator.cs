using System;
using System.Buffers;
using System.Linq;
using System.Security.Cryptography;

namespace Shortr
{
    public interface IKeyGenerator
    {
        string Generate();
    }

    public class KeyGenerator : IDisposable, IKeyGenerator
    {
        private readonly int _keyLength;
        private readonly RandomNumberGenerator _randomNumberGenerator;
        private const string Characters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

        public KeyGenerator() : this(7) { }
        public KeyGenerator(int keyLength) : this(keyLength, new RNGCryptoServiceProvider()) { }
        public KeyGenerator(int keyLength, RandomNumberGenerator randomNumberGenerator)
        {
            if (keyLength <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(keyLength), "Key length must be at least 1.");
            }
            _keyLength = keyLength;
            _randomNumberGenerator = randomNumberGenerator
                ?? throw new ArgumentNullException(nameof(randomNumberGenerator), "A rnd is required.");
        }

        public string Generate()
        {
            var higherBound = Characters.Length;

            #if NETSTANDARD2_0
            var randomBuffer = new byte[4];
            #else
            Span<byte> randomBuffer = stackalloc byte[4];
            #endif
            var stringBaseBuffer = ArrayPool<char>.Shared.Rent(_keyLength);
            try
            {
                for (var i = 0; i < _keyLength; i++)
                {
                    _randomNumberGenerator.GetBytes(randomBuffer);
#if NETSTANDARD2_0
                    var generatedValue = Math.Abs(BitConverter.ToInt32(randomBuffer, 0));
#else
                    var generatedValue = Math.Abs(BitConverter.ToInt32(randomBuffer));
#endif
                    var index = generatedValue % higherBound;
                    stringBaseBuffer[i] = Characters[index];
                }
#if NETSTANDARD2_0
                return new string(stringBaseBuffer.Take(_keyLength).ToArray());
#else
                return new string(stringBaseBuffer[.._keyLength]);
#endif
            }
            finally
            {
                ArrayPool<char>.Shared.Return(stringBaseBuffer);
            }
        }

        public void Dispose() => _randomNumberGenerator.Dispose();
    }
}
