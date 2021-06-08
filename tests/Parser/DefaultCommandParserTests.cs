using System;
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
                [Target] DefaultCommandParser parser,
                CancellationToken cancellationToken
            )
            {
                options.Prefix = '.';
                options.ArgSeparator = ' ';

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

                commandsClient.GetCommandParseInfo(Any<string>(), Any<ClientRequestOptions>(), Any<CancellationToken>()).Throws(new ApiException(string.Empty, 404, string.Empty, null, null));

                var message = ".echo Hello World";
                var result = await parser.ParseCommand(message, options, cancellationToken);

                result.Should().BeNull();
                await commandsClient.Received().GetCommandParseInfo(Is("echo"), Is<ClientRequestOptions>(requestOptions => requestOptions.ImpersonateUserId == options.ImpersonateUserId), Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldParseCommandArgumentsWhenPrefixed(
                CommandParserOptions options,
                [Target] DefaultCommandParser parser,
                CancellationToken cancellationToken
            )
            {
                options.Prefix = '.';
                options.ArgSeparator = ' ';

                var message = ".echo Hello World";
                var result = await parser.ParseCommand(message, options, cancellationToken);

                result!.Arguments.Should().BeEquivalentTo(new[] { "Hello", "World" });
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
                result!.Arguments.Should().BeEquivalentTo(Array.Empty<string>());
            }

            [Test, Auto]
            public async Task ShouldCombineArgumentsIntoTheLastArgIfArgLimitIsReached(
                CommandParserOptions options,
                [Frozen] CommandParseInfo parserRestrictions,
                [Target] DefaultCommandParser parser,
                CancellationToken cancellationToken
            )
            {
                options.Prefix = '.';
                options.ArgSeparator = ' ';
                parserRestrictions.ArgCount = 1;

                var message = ".echo This is a lot of arguments";
                var result = await parser.ParseCommand(message, options, cancellationToken);

                result!.Name.Should().Be("echo");
                result!.Arguments.Should().BeEquivalentTo(new[] { "This is a lot of arguments" });
            }

            [Test, Auto]
            public async Task ShouldParseOptions(
                CommandParserOptions options,
                [Frozen] CommandParseInfo parserRestrictions,
                [Target] DefaultCommandParser parser,
                CancellationToken cancellationToken
            )
            {
                options.Prefix = '.';
                options.ArgSeparator = ' ';
                options.OptionPrefix = "--";
                parserRestrictions.ArgCount = 1;
                parserRestrictions.ValidOptions = new[] { "hello", "foo" };

                var message = ".echo This is a lot of arguments --hello world --foo bar";
                var result = await parser.ParseCommand(message, options, cancellationToken);

                result.Should().NotBeNull();
                result!.Arguments.Should().BeEquivalentTo(new[] { "This is a lot of arguments" });
                result!.Options.Should().Contain("hello", "world");
                result!.Options.Should().Contain("foo", "bar");
            }

            [Test, Auto]
            public async Task ShouldReturnNullWhenOptionIsNotValid(
                CommandParserOptions options,
                [Frozen] CommandParseInfo parserRestrictions,
                [Frozen, Substitute] ICommandsClient commandsClient,
                [Target] DefaultCommandParser parser,
                CancellationToken cancellationToken
            )
            {
                options.Prefix = '.';
                options.ArgSeparator = ' ';
                options.OptionPrefix = "--";
                parserRestrictions.ValidOptions = new[] { "foo" };

                var message = ".echo Hello World --bar foo";
                var result = await parser.ParseCommand(message, options, cancellationToken);

                result.Should().BeNull();
                await commandsClient.Received().GetCommandParseInfo(Is("echo"), Is<ClientRequestOptions>(requestOptions => requestOptions.ImpersonateUserId == options.ImpersonateUserId), Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldReturnNullIfArgumentsExceedMaxArgsAroundOptions(
                CommandParserOptions options,
                [Frozen] CommandParseInfo parserRestrictions,
                [Target] DefaultCommandParser parser,
                CancellationToken cancellationToken
            )
            {
                options.Prefix = '.';
                options.ArgSeparator = ' ';
                options.OptionPrefix = "--";
                parserRestrictions.ArgCount = 1;

                var message = ".echo This is a lot of arguments --hello world --foo bar more arguments here";
                var result = await parser.ParseCommand(message, options, cancellationToken);

                result.Should().BeNull();
            }
        }
    }
}
