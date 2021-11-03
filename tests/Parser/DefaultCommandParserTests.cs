using System.Threading;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using FluentAssertions;

using NSubstitute;
using NSubstitute.ExceptionExtensions;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Brighid.Commands.Client.Parser
{
    internal class DefaultCommandParserTests
    {
        [TestFixture]
        public class TryParseCommandTests
        {
            [Test, Auto]
            public async Task ShouldReturnNullWhenNotPrefixed(
                CommandParserOptions options,
                [Target] DefaultCommandParser parser,
                CancellationToken cancellationToken
            )
            {
                options.Prefix = '.';
                options.ArgSeparator = ' ';

                var message = "echo Hello World";
                var result = await parser.ParseCommand(message, options, cancellationToken);

                result.Should().BeNull();
            }

            [Test, Auto]
            public async Task ShouldReturnNullWhenCommandNameDoesntStartWithALetter(
                CommandParserOptions options,
                [Target] DefaultCommandParser parser,
                CancellationToken cancellationToken
            )
            {
                options.Prefix = '.';
                options.ArgSeparator = ' ';

                var message = "..echo Hello World";
                var result = await parser.ParseCommand(message, options, cancellationToken);

                result.Should().BeNull();
            }

            [Test, Auto]
            public async Task ShouldParseCommandNameWhenPrefixed(
                CommandParserOptions options,
                [Frozen] ICommandsClient commandsClient,
                [Target] DefaultCommandParser parser,
                CancellationToken cancellationToken
            )
            {
                options.Prefix = '.';
                options.ArgSeparator = ' ';

                commandsClient.GetCommandParameters(Any<string>(), Any<ClientRequestOptions>(), Any<CancellationToken>()).Returns(new[]
                {
                    new CommandParameter { Name = "String", ArgumentIndex = 0 },
                });

                var message = ".echo Hello World";
                var result = await parser.ParseCommand(message, options, cancellationToken);

                result!.Name.Should().Be("echo");
            }

            [Test, Auto]
            public async Task ShouldReturnNullWhenCommandDoesntExist(
                CommandParserOptions options,
                [Frozen, Substitute] ICommandsClient commandsClient,
                [Target] DefaultCommandParser parser,
                CancellationToken cancellationToken
            )
            {
                options.Prefix = '.';
                options.ArgSeparator = ' ';

                commandsClient.GetCommandParameters(Any<string>(), Any<ClientRequestOptions>(), Any<CancellationToken>()).Throws(new ApiException(string.Empty, 404, string.Empty, null, null));

                var message = ".echo Hello World";
                var result = await parser.ParseCommand(message, options, cancellationToken);

                result.Should().BeNull();
                await commandsClient.Received().GetCommandParameters(Is("echo"), Is<ClientRequestOptions>(requestOptions => requestOptions.ImpersonateUserId == options.ImpersonateUserId), Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldParseCommandArgumentsWhenPrefixed(
                CommandParserOptions options,
                [Frozen] ICommandsClient commandsClient,
                [Target] DefaultCommandParser parser,
                CancellationToken cancellationToken
            )
            {
                options.Prefix = '.';
                options.ArgSeparator = ' ';

                commandsClient.GetCommandParameters(Any<string>(), Any<ClientRequestOptions>(), Any<CancellationToken>()).Returns(new[]
                {
                    new CommandParameter { Name = "String1", ArgumentIndex = 0 },
                    new CommandParameter { Name = "String2", ArgumentIndex = 1 },
                });

                var message = ".echo Hello World";
                var result = await parser.ParseCommand(message, options, cancellationToken);

                result!.Parameters["String1"].Should().Be("Hello");
                result!.Parameters["String2"].Should().Be("World");
            }

            [Test, Auto]
            public async Task ShouldParseCommandsWithNoArguments(
                CommandParserOptions options,
                [Target] DefaultCommandParser parser,
                CancellationToken cancellationToken
            )
            {
                options.Prefix = '.';
                options.ArgSeparator = ' ';

                var message = ".ping";
                var result = await parser.ParseCommand(message, options, cancellationToken);

                result!.Name.Should().Be("ping");
                result!.Parameters.Should().BeEmpty();
            }

            [Test, Auto]
            public async Task ShouldCombineArgumentsIntoTheLastArgIfArgLimitIsReached(
                CommandParserOptions options,
                [Frozen] ICommandsClient commandsClient,
                [Target] DefaultCommandParser parser,
                CancellationToken cancellationToken
            )
            {
                options.Prefix = '.';
                options.ArgSeparator = ' ';

                commandsClient.GetCommandParameters(Any<string>(), Any<ClientRequestOptions>(), Any<CancellationToken>()).Returns(new[]
                {
                    new CommandParameter { Name = "String", ArgumentIndex = 0 },
                });

                var message = ".echo This is a lot of arguments";
                var result = await parser.ParseCommand(message, options, cancellationToken);

                result!.Name.Should().Be("echo");
                result!.Parameters["String"].Should().Be("This is a lot of arguments");
            }

            [Test, Auto]
            public async Task ShouldParseOptions(
                CommandParserOptions options,
                [Frozen] ICommandsClient commandsClient,
                [Target] DefaultCommandParser parser,
                CancellationToken cancellationToken
            )
            {
                options.Prefix = '.';
                options.ArgSeparator = ' ';
                options.OptionPrefix = "--";

                commandsClient.GetCommandParameters(Any<string>(), Any<ClientRequestOptions>(), Any<CancellationToken>()).Returns(new[]
                {
                    new CommandParameter { Name = "String", ArgumentIndex = 0 },
                    new CommandParameter { Name = "Hello" },
                    new CommandParameter { Name = "Foo" },
                });

                var message = ".echo This is a lot of arguments --hello world --foo bar";
                var result = await parser.ParseCommand(message, options, cancellationToken);

                result.Should().NotBeNull();
                result!.Parameters["String"].Should().Be("This is a lot of arguments");
                result!.Parameters["Hello"].Should().Be("world");
                result!.Parameters["Foo"].Should().Be("bar");
            }

            [Test, Auto]
            public async Task ShouldReturnNullWhenOptionIsNotValid(
                CommandParserOptions options,
                [Frozen, Substitute] ICommandsClient commandsClient,
                [Target] DefaultCommandParser parser,
                CancellationToken cancellationToken
            )
            {
                options.Prefix = '.';
                options.ArgSeparator = ' ';
                options.OptionPrefix = "--";

                commandsClient.GetCommandParameters(Any<string>(), Any<ClientRequestOptions>(), Any<CancellationToken>()).Returns(new[]
                {
                    new CommandParameter { Name = "String", ArgumentIndex = 0 },
                    new CommandParameter { Name = "Foo" },
                });

                var message = ".echo Hello World --bar foo";
                var result = await parser.ParseCommand(message, options, cancellationToken);

                result.Should().BeNull();
                await commandsClient.Received().GetCommandParameters(Is("echo"), Is<ClientRequestOptions>(requestOptions => requestOptions.ImpersonateUserId == options.ImpersonateUserId), Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldReturnNullIfArgumentsExceedMaxArgsAroundOptions(
                CommandParserOptions options,
                [Frozen] ICommandsClient commandsClient,
                [Target] DefaultCommandParser parser,
                CancellationToken cancellationToken
            )
            {
                options.Prefix = '.';
                options.ArgSeparator = ' ';
                options.OptionPrefix = "--";

                commandsClient.GetCommandParameters(Any<string>(), Any<ClientRequestOptions>(), Any<CancellationToken>()).Returns(new[]
                {
                    new CommandParameter { Name = "String", ArgumentIndex = 0 },
                    new CommandParameter { Name = "Hello" },
                    new CommandParameter { Name = "Foo" },
                });

                var message = ".echo This is a lot of arguments --hello world --foo bar more arguments here";
                var result = await parser.ParseCommand(message, options, cancellationToken);

                result.Should().BeNull();
            }
        }
    }
}
