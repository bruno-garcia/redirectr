using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Redirectr
{
    public readonly struct RegistrationOptions
    {
        public RegistrationOptions(string key, string url, int? ttl)
        {
            Key = key;
            Url = url;
            Ttl = ttl;
        }

        public string Key { get; }
        public string Url { get; }
        public int? Ttl { get; }
    }

    public interface IRedirectrStore
    {
        ValueTask<bool> TryGetUrl(string key, [NotNullWhen(true)] out string? url);
        ValueTask<bool> TryGetKey(string url, [NotNullWhen(true)] out string? key);
        ValueTask RegisterUrl(in RegistrationOptions options);
    }
}
