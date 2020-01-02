using System;
using System.Buffers;
using System.Security.Cryptography;

namespace Redirectr.Web
{
    public class KeyGenerator : IDisposable
    {
        private readonly RandomNumberGenerator _randomNumberGenerator;
        private const string Characters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

        public KeyGenerator(RandomNumberGenerator? randomNumberGenerator = null)
            => _randomNumberGenerator = randomNumberGenerator ?? new RNGCryptoServiceProvider();

        public string Generate()
        {
            var higherBound = Characters.Length;

            const int keyLength = 7;
            Span<byte> randomBuffer = stackalloc byte[4];
            var stringBaseBuffer = ArrayPool<char>.Shared.Rent(keyLength);
            try
            {
                for (var i = 0; i < keyLength; i++)
                {
                    _randomNumberGenerator.GetBytes(randomBuffer);
                    var generatedValue = Math.Abs(BitConverter.ToInt32(randomBuffer));
                    var index = generatedValue % higherBound;
                    stringBaseBuffer[i] = Characters[index];
                }

                return new string(stringBaseBuffer[..keyLength]);
            }
            finally
            {
                ArrayPool<char>.Shared.Return(stringBaseBuffer);
            }
        }

        public void Dispose() => _randomNumberGenerator.Dispose();
    }
}