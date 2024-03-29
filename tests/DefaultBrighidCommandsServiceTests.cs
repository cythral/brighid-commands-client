using System.Threading;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Commands.Client.Parser;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Brighid.Commands.Client
{
    public class DefaultBrighidCommandsServiceTests
    {
        [TestFixture]
        [Category("Unit")]
        public class ParseAndExecuteCommandAsUserTests
        {
            [Test, Auto]
            public async Task ShouldParseWithConfiguredDefaultPrefix(
                string message,
                string userId,
                string sourceSystemChannel,
                string sourceSystemUser,
                [Frozen] Parser.Command command,
                [Frozen] ICommandsClient commandsClient,
                [Frozen] CommandsClientOptions options,
                [Frozen, Substitute] ICommandParser parser,
                [Target] DefaultBrighidCommandsService client,
                CancellationToken cancellationToken
            )
            {
                await client.ParseAndExecuteCommandAsUser(message, userId, sourceSystemChannel, sourceSystemUser, cancellationToken);

                await parser.Received().ParseCommand(Is(message), Is<CommandParserOptions>(parserOptions => parserOptions.Prefix == options.DefaultPrefix), Is(cancellationToken));
                await commandsClient.Received().ExecuteCommand(
                    Is(command.Name),
                    Is<ExecuteCommandRequest>(req => req.AdditionalProperties == command.Parameters),
                    Is<ClientRequestOptions>(requestOptions =>
                        requestOptions.ImpersonateUserId == userId &&
                        requestOptions.SourceSystem == options.SystemName &&
                        requestOptions.SourceSystemChannel == sourceSystemChannel &&
                        requestOptions.SourceSystemUser == sourceSystemUser
                    ),
                    Is(cancellationToken)
                );
            }
        }
    }
}
