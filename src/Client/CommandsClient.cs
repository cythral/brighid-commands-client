using System.Threading;
using System.Threading.Tasks;

using Brighid.Commands.Client.Parser;

namespace Brighid.Commands.Client
{
    /// <inheritdoc />
    public partial class CommandsClient : ICommandsClient
    {
        private ICommandParser? parser;

        /// <inheritdoc />
        public async Task<Command?> ParseCommandAsUser(string message, string userId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            parser ??= new DefaultCommandParser(this);
            var options = new CommandParserOptions { ImpersonateUserId = userId };
            return await parser.ParseCommand(message, options, cancellationToken);
        }
    }
}
