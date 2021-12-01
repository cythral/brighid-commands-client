using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Extensions.Caching.Memory;

namespace Brighid.Commands.Client
{
    /// <inheritdoc />
    public class DefaultBrighidCommandsCache : MemoryCache, IBrighidCommandsCache
    {
        private const string ParametersPrefix = "Parameters.";

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultBrighidCommandsCache" /> class.
        /// </summary>
        public DefaultBrighidCommandsCache()
            : base(new MemoryCacheOptions { SizeLimit = 1000 })
        {
        }

        /// <inheritdoc />
        public void ClearParameters(string name)
        {
            Remove($"{ParametersPrefix}{name}");
        }

        /// <inheritdoc />
        public Task<ICollection<CommandParameter>> GetOrCreateAsync(string name, Func<ICacheEntry, Task<ICollection<CommandParameter>>> factory)
        {
            return (this as IMemoryCache).GetOrCreateAsync($"{ParametersPrefix}{name}", factory);
        }
    }
}
