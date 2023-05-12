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
        /// Gets a value indicating the number of commands that have cached parameters.
        /// </summary>
        int ParametersCacheCount { get; }

        /// <summary>
        /// Clears out all cached parameters.
        /// </summary>
        void ClearAllParameters();

        /// <summary>
        /// Clears out parameters cached for a specific command by name.
        /// </summary>
        /// <param name="name">The name of the command to clear out parameters for.</param>
        void ClearParameters(string name);

        /// <summary>
        /// Checks to see if a commands' parameters are cached or not.
        /// </summary>
        /// <param name="name">The name of the command to check if its parameters are cached.</param>
        /// <returns>True if the parameters are cached, or false if not.</returns>
        bool ParametersExist(string name);

        /// <summary>
        /// Gets cached command parameters or creates new ones.
        /// </summary>
        /// <param name="name">The name of the command to cache parameters for.</param>
        /// <param name="factory">Factory for creating a new cache entry.</param>
        /// <returns>The resulting command parameters.</returns>
        Task<ICollection<CommandParameter>> GetOrCreateParametersAsync(string name, Func<ICacheEntry, Task<ICollection<CommandParameter>>> factory);
    }
}
