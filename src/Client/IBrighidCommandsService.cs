using System.Threading;
using System.Threading.Tasks;

namespace Brighid.Commands.Client
{
    /// <summary>
    /// Client used to interact with the Commands Service API.
    /// </summary>
    public interface IBrighidCommandsService
    {
        /// <summary>
        /// Parses a command as a user with the given <paramref name="userId" />. (The user is impersonated by the client
        /// application when retrieving command info).
        /// </summary>
        /// <param name="message">The message to parse a command from.</param>
        /// <param name="userId">ID of the user to impersonate.</param>
        /// <param name="sourceSystemChannel">ID of the channel in the source system that is executing the command.</param>
        /// <param name="sourceSystemUser">ID of the user in the source system that is executing the command.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting command, or null if there was an error.</returns>
        Task<ExecuteCommandResponse?> ParseAndExecuteCommandAsUser(string message, string userId, string sourceSystemChannel, string sourceSystemUser, CancellationToken cancellationToken);
    }
}
