using Brighid.Commands.Client.Parser;

namespace Brighid.Commands.Client
{
    /// <summary>
    /// Factory for creating command parsers.
    /// </summary>
    public interface ICommandParserFactory
    {
        /// <summary>
        /// Creates a new command parser.
        /// </summary>
        /// <param name="commandsClient">The command client to use.</param>
        /// <returns>The resulting command parser.</returns>
        ICommandParser CreateParser(ICommandsClient commandsClient);
    }
}
