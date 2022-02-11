namespace Brighid.Commands.Client
{
    /// <summary>
    /// Options to use for client operations.
    /// </summary>
    public readonly struct ClientRequestOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClientRequestOptions" /> struct.
        /// </summary>
        /// <param name="impersonateUserId">User id to impersonate for the request.</param>
        /// <param name="sourceSystem">Name of the source system the request came from.</param>
        /// <param name="sourceSystemId">ID of the channel/sender the request came from.</param>
        public ClientRequestOptions(
            string? impersonateUserId = null,
            string? sourceSystem = null,
            string? sourceSystemId = null
        )
        {
            ImpersonateUserId = impersonateUserId;
            SourceSystem = sourceSystem ?? string.Empty;
            SourceSystemId = sourceSystemId ?? string.Empty;
        }

        /// <summary>
        /// Gets the userId to impersonate.
        /// </summary>
        public string? ImpersonateUserId { get; init; }

        /// <summary>
        /// Gets the name of the source system the sender made a request from.
        /// </summary>
        public string SourceSystem { get; init; }

        /// <summary>
        /// Gets the source system's id of the channel/sender who is making the request.
        /// </summary>
        public string SourceSystemId { get; init; }
    }
}
