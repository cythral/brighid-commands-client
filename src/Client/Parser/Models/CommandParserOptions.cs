namespace Brighid.Commands.Client.Parser
{
    /// <summary>
    /// Options to use when parsing messages as commands.
    /// </summary>
    public class CommandParserOptions
    {
        /// <summary>
        /// Gets or sets the prefix to use for commands.
        /// </summary>
        public char Prefix { get; set; } = '.';

        /// <summary>
        /// Gets or sets the options to use when making requests to the commands service.
        /// </summary>
        public ClientRequestOptions ClientRequestOptions { get; set; }

        /// <summary>
        /// Gets or sets the argument separator.
        /// </summary>
        public char ArgSeparator { get; set; } = ' ';

        /// <summary>
        /// Gets or sets the prefix used for option flags.
        /// </summary>
        public string OptionPrefix { get; set; } = "--";
    }
}
