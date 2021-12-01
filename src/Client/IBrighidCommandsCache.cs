using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Extensions.Caching.Memory;

namespace Brighid.Commands.Client
{
    /// <summary>
    /// Cache for command parameters.
    /// </summary>
    public interface IBrighidCommandsCache
    {
        /// <summary>
        /// Clears out parameters cached for a specific command by name.
        /// </summary>
        /// <param name="name">The name of the command to clear out parameters for.</param>
        void ClearParameters(string name);

        /// <summary>
        /// Gets cached command parameters or creates new ones.
        /// </summary>
        /// <param name="name">The name of the command to cache parameters for.</param>
        /// <param name="factory">Factory for creating a new cache entry.</param>
        /// <returns>The resulting command parameters.</returns>
        Task<ICollection<CommandParameter>> GetOrCreateAsync(string name, Func<ICacheEntry, Task<ICollection<CommandParameter>>> factory);
    }
}
