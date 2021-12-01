using System.Threading;
using System.Threading.Tasks;

namespace Brighid.Commands.Client.Parser
{
    /// <inheritdoc />
    internal class DefaultCommandParser : ICommandParser
    {
        private readonly ICommandsClient commandsClient;
        private readonly IBrighidCommandsCache cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultCommandParser" /> class.
        /// </summary>
        /// <param name="commandsClient">The client to use for the commands service.</param>
        /// <param name="cache">Cache for command parameter responses.</param>
        public DefaultCommandParser(
            ICommandsClient commandsClient,
            IBrighidCommandsCache cache
        )
        {
            this.commandsClient = commandsClient;
            this.cache = cache;
        }

        /// <inheritdoc />
        public async Task<Command?> ParseCommand(string message, CommandParserOptions options, CancellationToken cancellationToken)
        {
            var stateMachine = new CommandParserStateMachine(commandsClient, cache, options);
            await stateMachine.Run(message, cancellationToken);
            return stateMachine.Success ? stateMachine.Result : null;
        }
    }
}
