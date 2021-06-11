using System.Threading;
using System.Threading.Tasks;

namespace Brighid.Commands.Client.Parser
{
    /// <inheritdoc />
    internal class DefaultCommandParser : ICommandParser
    {
        private readonly ICommandsClient commandsClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultCommandParser" /> class.
        /// </summary>
        /// <param name="commandsClient">The client to use for the commands service.</param>
        public DefaultCommandParser(
            ICommandsClient commandsClient
        )
        {
            this.commandsClient = commandsClient;
        }

        /// <inheritdoc />
        public async Task<Command?> ParseCommand(string message, CommandParserOptions options, CancellationToken cancellationToken)
        {
            var stateMachine = new CommandParserStateMachine(commandsClient, options);
            await stateMachine.Run(message, cancellationToken);
            return stateMachine.Success ? stateMachine.Result : null;
        }
    }
}
