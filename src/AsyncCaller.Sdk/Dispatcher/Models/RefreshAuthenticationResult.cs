using System.Collections.Generic;

namespace Nexus.Link.AsyncCaller.Sdk.Dispatcher.Models
{
    /// <summary>
    /// Response from originator when refreshing authentication
    /// </summary>
    public class RefreshAuthenticationResult
    {
        /// <summary>
        /// The authorization headers to use for the new request execution.
        /// </summary>
        /// <remarks>
        /// Usually only the "Authorization" header
        /// </remarks>
        public List<RefreshAuthenticationHeader> Headers { get; set; }
    }
}
