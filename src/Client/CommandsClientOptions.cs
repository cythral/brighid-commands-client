using System;

namespace Brighid.Commands.Client
{
    /// <summary>
    /// Options to use for the commands client.
    /// </summary>
    public class CommandsClientOptions
    {
        /// <summary>
        /// Gets or sets the default prefix used when parsing commands.
        /// </summary>
        public char DefaultPrefix { get; set; } = default;

        /// <summary>
        /// Gets or sets the Service URI where Brighid Commands Lives.
        /// </summary>
        public Uri ServiceUri { get; set; } = new Uri("https://commands.brigh.id");

        /// <summary>
        /// Gets or sets the name of the system this client is being used on (this gets sent as the x-source-system header).
        /// </summary>
        public string SystemName { get; set; } = string.Empty;
    }
}
