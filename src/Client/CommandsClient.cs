using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Commands.Client.Parser;

using Microsoft.Extensions.Options;

namespace Brighid.Commands.Client
{
    /// <inheritdoc />
    public partial class CommandsClient : ICommandsClient
    {
        private readonly ICommandParser parser;
        private readonly CommandsClientOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandsClient" /> class.
        /// </summary>
        /// <param name="httpClient">The HTTP Client to use for making API calls.</param>
        /// <param name="parserFactory">Factory used to create parsers with.</param>
        /// <param name="options">Options to use for the commands client.</param>
        public CommandsClient(
            HttpClient httpClient,
            ICommandParserFactory parserFactory,
            IOptions<CommandsClientOptions> options
        )
            : this(httpClient)
        {
            parser = parserFactory.CreateParser(this);
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
            var executeCommandRequest = new ExecuteCommandRequest { Options = command.Options, Arguments = command.Arguments };
            return await ExecuteCommand(command!.Name, executeCommandRequest, executeCommandOptions, cancellationToken);
        }
    }
}
