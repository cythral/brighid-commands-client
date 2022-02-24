using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Caching.Memory;

namespace Brighid.Commands.Client
{
    /// <inheritdoc />
    public class DefaultBrighidCommandsCache : MemoryCache, IBrighidCommandsCache
    {
        private const string ParametersPrefix = "Parameters";
        private const char Delimiter = '.';
        private readonly List<string> cachedParameters = new();
        private readonly PostEvictionCallbackRegistration evictionRegistration;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultBrighidCommandsCache" /> class.
        /// </summary>
        public DefaultBrighidCommandsCache()
            : base(new MemoryCacheOptions { SizeLimit = 1000 })
        {
            evictionRegistration = new(EvictionCallback);
        }

        /// <inheritdoc />
        public void ClearAllParameters()
        {
            foreach (var cachedParameter in cachedParameters.ToList())
            {
                Remove(cachedParameter);
            }
        }

        /// <inheritdoc />
        public bool ParametersExist(string name)
        {
            return TryGetValue($"{ParametersPrefix}{Delimiter}{name}", out _);
        }

        /// <inheritdoc />
        public void ClearParameters(string name)
        {
            Remove($"{ParametersPrefix}{Delimiter}{name}");
        }

        /// <inheritdoc />
        public Task<ICollection<CommandParameter>> GetOrCreateParametersAsync(string name, Func<ICacheEntry, Task<ICollection<CommandParameter>>> factory)
        {
            var parameterKey = $"{ParametersPrefix}{Delimiter}{name}";
            return (this as IMemoryCache).GetOrCreateAsync(parameterKey, (ICacheEntry entry) =>
            {
                cachedParameters.Add(parameterKey);
                entry.PostEvictionCallbacks.Add(evictionRegistration);
                return factory(entry);
            });
        }

        private void EvictionCallback(object key, object value, EvictionReason reason, object state)
        {
            var keyString = (string)key;
            var prefix = keyString.Split(Delimiter)[0];

            switch (prefix)
            {
                case ParametersPrefix: cachedParameters.Remove(keyString); break;
                default: break;
            }
        }

        private class PostEvictionCallbackRegistration : Microsoft.Extensions.Caching.Memory.PostEvictionCallbackRegistration
        {
            public PostEvictionCallbackRegistration(PostEvictionDelegate evictionCallback)
            {
                EvictionCallback = evictionCallback;
            }
        }
    }
}
