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
    }
}
