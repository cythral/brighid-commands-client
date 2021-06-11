using System.Collections.Generic;

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
            ServiceCollection services
        )
        {
            var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["Commands:DefaultPrefix"] = $"{prefix}",
            })
            .Build();

            services.ConfigureBrighidCommands(options => configuration.Bind("Commands", options));
            var provider = services.BuildServiceProvider();
            var options = provider.GetRequiredService<IOptions<CommandsClientOptions>>();

            options.Value.DefaultPrefix.Should().Be(prefix);
        }

        [Test, Auto]
        public void ShouldAddCommandParserFactorySingleton(
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

            services.ConfigureBrighidCommands(options => configuration.Bind("Commands", options));
            var provider = services.BuildServiceProvider();
            var parserFactory1 = provider.GetRequiredService<ICommandParserFactory>();
            var parserFactory2 = provider.GetRequiredService<ICommandParserFactory>();

            parserFactory1.Should().BeSameAs(parserFactory2);
        }
    }
}
