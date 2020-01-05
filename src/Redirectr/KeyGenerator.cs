using System;
using System.Buffers;
using System.Security.Cryptography;

namespace Redirectr
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

        public KeyGenerator(int keyLength = 7, RandomNumberGenerator? randomNumberGenerator = null)
        {
            if (keyLength <= 0) throw new ArgumentOutOfRangeException(nameof(keyLength), "Key length must be at least 1.");
            _keyLength = keyLength;
            _randomNumberGenerator = randomNumberGenerator ?? new RNGCryptoServiceProvider();
        }

        public string Generate()
        {
            var higherBound = Characters.Length;

            Span<byte> randomBuffer = stackalloc byte[4];
            var stringBaseBuffer = ArrayPool<char>.Shared.Rent(_keyLength);
            try
            {
                for (var i = 0; i < _keyLength; i++)
                {
                    _randomNumberGenerator.GetBytes(randomBuffer);
                    var generatedValue = Math.Abs(BitConverter.ToInt32(randomBuffer));
                    var index = generatedValue % higherBound;
                    stringBaseBuffer[i] = Characters[index];
                }

                return new string(stringBaseBuffer[.._keyLength]);
            }
            finally
            {
                ArrayPool<char>.Shared.Return(stringBaseBuffer);
            }
        }

        public void Dispose() => _randomNumberGenerator.Dispose();
    }
}
