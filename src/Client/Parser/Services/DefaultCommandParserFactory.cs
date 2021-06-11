using Brighid.Commands.Client.Parser;

namespace Brighid.Commands.Client
{
    /// <inheritdoc />
    public class DefaultCommandParserFactory : ICommandParserFactory
    {
        /// <summary>
        /// Creates a new command parser.
        /// </summary>
        /// <param name="commandsClient">The commands client to use when looking up restrictions.</param>
        /// <returns>The resulting parser.</returns>
        public ICommandParser CreateParser(ICommandsClient commandsClient)
        {
            return new DefaultCommandParser(commandsClient);
        }
    }
}
