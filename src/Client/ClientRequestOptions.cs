namespace Brighid.Commands.Client
{
    /// <summary>
    /// Options to use for client operations.
    /// </summary>
    public struct ClientRequestOptions
    {
        /// <summary>
        /// Gets or sets the userId to impersonate.
        /// </summary>
        public string? ImpersonateUserId { get; set; }

        /// <summary>
        /// Gets or sets the name of the source system the sender made a request from.
        /// </summary>
        public string SourceSystem { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the source system's id of the channel/sender who is making the request.
        /// </summary>
        public string SourceSystemId { get; set; } = string.Empty;
    }
}
