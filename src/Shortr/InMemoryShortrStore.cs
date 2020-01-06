using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Shortr
{
    public class InMemoryShortrStore : IShortrStore
    {
        private readonly ConcurrentDictionary<string, string> _urls = new ConcurrentDictionary<string, string>();

        public ValueTask<string?> GetKey(string url, CancellationToken token)
        {
            return _urls.FirstOrDefault(u => u.Value == url)
                       is {} pair && pair.Key is {}
                ? new ValueTask<string?>(pair.Key)
                : new ValueTask<string?>(null as string);
        }

        public ValueTask<string?> GetUrl(string key, CancellationToken token)
            => new ValueTask<string?>(_urls.TryGetValue(key, out var url) ? url : null);

        public ValueTask RegisterUrl(RegistrationOptions options, CancellationToken token)
        {
            // TODO: support TTL
            _urls.TryAdd(options.Key, options.Url);
            return new ValueTask();
        }
    }
}
