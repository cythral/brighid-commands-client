using System;

using Brighid.Commands.Client;
using Brighid.Commands.Client.Parser;

using Microsoft.Extensions.DependencyInjection.Extensions;

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
        public static void AddBrighidCommands(this IServiceCollection services, Action<CommandsClientOptions> configure)
        {
            var options = new CommandsClientOptions();
            configure(options);

            services.AddOptions<CommandsClientOptions>().Configure(configure);
            services.TryAddSingleton<ICommandParser, DefaultCommandParser>();
            services.TryAddSingleton<IBrighidCommandsService, DefaultBrighidCommandsService>();
            services.TryAddSingleton<IBrighidCommandsCache, DefaultBrighidCommandsCache>();
            services.UseBrighidCommands(options.ServiceUri);
        }
    }
}
