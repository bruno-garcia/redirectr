using System.Threading;
using System.Threading.Tasks;

namespace Shortr
{
    public interface IShortrStore
    {
        ValueTask<string?> GetUrl(string key, CancellationToken token);
        ValueTask<string?> GetKey(string url, CancellationToken token);
        ValueTask RegisterUrl(RegistrationOptions options, CancellationToken token);
    }
}
