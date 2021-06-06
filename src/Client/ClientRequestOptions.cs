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
    }
}
