using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Commands.Client.Parser;

using NSubstitute;

using NUnit.Framework;

using RichardSzalay.MockHttp;

using static NSubstitute.Arg;

namespace Brighid.Commands.Client
{
    public class CommandsClientTests
    {
        [TestFixture]
        [Category("Unit")]
        public class ParseAndExecuteCommandAsUserTests
        {
            [Test, Auto]
            public async Task ShouldParseWithConfiguredDefaultPrefix(
                string message,
                string userId,
                [Frozen] MockHttpMessageHandler handler,
                [Frozen] CommandsClientOptions options,
                [Frozen, Substitute] ICommandParser parser,
                [Target] CommandsClient client,
                CancellationToken cancellationToken
            )
            {
                handler
                .Expect(HttpMethod.Post, "http://localhost/commands/*/execute")
                .Respond(HttpStatusCode.OK, "application/json", "{}");

                await client.ParseAndExecuteCommandAsUser(message, userId, cancellationToken);

                await parser.Received().ParseCommand(Is(message), Is<CommandParserOptions>(parserOptions => parserOptions.Prefix == options.DefaultPrefix), Is(cancellationToken));

                handler.VerifyNoOutstandingExpectation();
            }
        }
    }
}
