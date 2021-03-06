using System.Collections.Generic;

namespace Brighid.Commands.Client.Parser
{
    /// <summary>
    /// Represents a command that a user sent as a message.
    /// </summary>
    public class Command
    {
        /// <summary>
        /// Gets or sets the command name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the command options.
        /// </summary>
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
    }
}
