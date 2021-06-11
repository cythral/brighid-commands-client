using System;

using Brighid.Commands.Client;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extensions for configuring services for Brighid Commands.
    /// </summary>
    public static class CommandsServiceCollectionExtensions
    {
        /// <summary>
        /// Configures Brighid Commands.
        /// </summary>
        /// <param name="services">The services to configure.</param>
        /// <param name="configure">The configuration delegate used to configure Brighid Commands.</param>
        public static void ConfigureBrighidCommands(this IServiceCollection services, Action<CommandsClientOptions> configure)
        {
            services.AddOptions<CommandsClientOptions>().Configure(configure);
            services.AddSingleton<ICommandParserFactory, DefaultCommandParserFactory>();
        }
    }
}
