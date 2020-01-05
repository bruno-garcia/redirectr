using System.Threading;
using System.Threading.Tasks;

namespace Redirectr
{
    public interface IRedirectrStore
    {
        ValueTask<string?> GetUrl(string key, CancellationToken token);
        ValueTask<string?> GetKey(string url, CancellationToken token);
        ValueTask RegisterUrl(RegistrationOptions options, CancellationToken token);
    }
}
