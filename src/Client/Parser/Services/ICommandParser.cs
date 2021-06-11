using System.Threading;
using System.Threading.Tasks;

namespace Brighid.Commands.Client.Parser
{
    /// <summary>
    /// Parser that parses messages into commands.
    /// </summary>
    public interface ICommandParser
    {
        /// <summary>
        /// Tries to parse a gateway message into a command.
        /// </summary>
        /// <param name="message">The message to parse.</param>
        /// <param name="options">Options to use for parsing messages into commands.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting command if parse was successful.</returns>
        Task<Command?> ParseCommand(string message, CommandParserOptions options, CancellationToken cancellationToken);
    }
}
