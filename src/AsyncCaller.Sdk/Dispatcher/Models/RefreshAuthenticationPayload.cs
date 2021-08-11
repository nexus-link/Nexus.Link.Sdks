namespace Nexus.Link.AsyncCaller.Sdk.Dispatcher.Models
{
    /// <summary>
    /// Payload when Async Caller requests new authentication from originating client
    /// </summary>
    public class RefreshAuthenticationPayload
    {
        /// <summary>
        /// The initial Authorization header
        /// </summary>
        public string ExpiredAuthorizationHeader { get; set; }

        /// <summary>
        /// The value from Metadata.Authentication.Context from the original Request
        /// </summary>
        public string AuthenticationContext { get; set; }
    }
}
