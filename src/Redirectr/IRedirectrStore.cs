using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Redirectr
{
    public interface IRedirectrStore
    {
        ValueTask<bool> TryGetUrl(string key, [NotNullWhen(true)] out string? url);
        ValueTask<bool> TryGetKey(string url, [NotNullWhen(true)] out string? key);
        ValueTask RegisterUrl(in RegistrationOptions options);
    }
}
