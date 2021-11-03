using System.Threading;
using System.Threading.Tasks;

using Brighid.Commands.Client.Parser;

using Microsoft.Extensions.Options;

namespace Brighid.Commands.Client
{
    /// <inheritdoc />
    public class DefaultBrighidCommandsService : IBrighidCommandsService
    {
        private readonly ICommandsClient commandsClient;
        private readonly ICommandParser parser;
        private readonly CommandsClientOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultBrighidCommandsService" /> class.
        /// </summary>
        /// <param name="commandsClient">The HTTP Client to use for making API calls.</param>
        /// <param name="parser">Used to parse commands with.</param>
        /// <param name="options">Options to use for the commands client.</param>
        public DefaultBrighidCommandsService(
            ICommandsClient commandsClient,
            ICommandParser parser,
            IOptions<CommandsClientOptions> options
        )
        {
            this.commandsClient = commandsClient;
            this.parser = parser;
            this.options = options.Value;
        }

        /// <inheritdoc />
        public async Task<ExecuteCommandResponse?> ParseAndExecuteCommandAsUser(string message, string userId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var parserOptions = new CommandParserOptions
            {
                Prefix = options.DefaultPrefix,
                ImpersonateUserId = userId,
            };

            var command = await parser.ParseCommand(message, parserOptions, cancellationToken);
            if (command == null)
            {
                return null;
            }

            var executeCommandOptions = new ClientRequestOptions { ImpersonateUserId = userId };
            var executeCommandRequest = new ExecuteCommandRequest { AdditionalProperties = command.Parameters };
            return await commandsClient.ExecuteCommand(command!.Name, executeCommandRequest, executeCommandOptions, cancellationToken);
        }
    }
}
