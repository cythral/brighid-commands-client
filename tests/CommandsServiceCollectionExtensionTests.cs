using System;
using System.Collections.Generic;

using Brighid.Commands.Client.Parser;

using FluentAssertions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using NUnit.Framework;

namespace Brighid.Commands.Client
{
    [TestFixture]
    [Category("Unit")]
    public class CommandsServiceCollectionExtensionTests
    {
        [Test, Auto]
        public void ShouldConfigureTheDefaultPrefix(
            char prefix,
            Uri serviceUri,
            ServiceCollection services
        )
        {
            var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["Commands:DefaultPrefix"] = $"{prefix}",
                ["Commands:ServiceUri"] = serviceUri.ToString(),
            })
            .Build();

            services.AddSingleton(configuration);
            services.AddBrighidCommands(options => configuration.Bind("Commands", options));
            var provider = services.BuildServiceProvider();
            var options = provider.GetRequiredService<IOptions<CommandsClientOptions>>();

            options.Value.DefaultPrefix.Should().Be(prefix);
        }

        [Test, Auto]
        public void ShouldAddCommandParserSingleton(
            char prefix,
            ServiceCollection services
        )
        {
            var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["Commands:DefaultPrefix"] = $"{prefix}",
            })
            .Build();

            services.AddBrighidCommands(options => configuration.Bind("Commands", options));
            var provider = services.BuildServiceProvider();
            var parser1 = provider.GetRequiredService<ICommandParser>();
            var parser2 = provider.GetRequiredService<ICommandParser>();

            parser1.Should().BeSameAs(parser2);
        }

        [Test, Auto]
        public void ShouldAddCommandsClient(
            char prefix,
            ServiceCollection services
        )
        {
            var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["Commands:DefaultPrefix"] = $"{prefix}",
            })
            .Build();

            services.AddBrighidCommands(options => configuration.Bind("Commands", options));
            services.UseBrighidCommands(new("http://localhost/"));
            var provider = services.BuildServiceProvider();
            var commandsClient = provider.GetRequiredService<IBrighidCommandsService>();

            commandsClient.Should().NotBeNull();
        }
    }
}
