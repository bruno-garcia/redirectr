using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Redirectr
{
    public class InMemoryRedirectrStore : IRedirectrStore
    {
        private readonly ConcurrentDictionary<string, string> _urls = new ConcurrentDictionary<string, string>();

        public ValueTask<bool> TryGetUrl(string key, [NotNullWhen(true)] out string? url)
            => new ValueTask<bool>(_urls.TryGetValue(key, out url));

        public ValueTask<bool> TryGetKey(string url, [NotNullWhen(true)] out string? key)
        {
            if (_urls.FirstOrDefault(u => u.Value == url) is {} pair && pair.Key is {})
            {
                key = pair.Key;
                return new ValueTask<bool>(true);
            }

            key = null;
            return new ValueTask<bool>(false);
        }

        public ValueTask RegisterUrl(in RegistrationOptions options)
        {
            // TODO: support TTL
            _urls.TryAdd(options.Key, options.Url);
            return new ValueTask();
        }
    }
}
