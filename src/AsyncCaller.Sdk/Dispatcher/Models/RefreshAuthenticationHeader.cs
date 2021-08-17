using System.Collections.Generic;

namespace Nexus.Link.AsyncCaller.Sdk.Dispatcher.Models
{
    /// <summary>
    /// Represents a HTTP header key/value pair
    /// </summary>
    public class RefreshAuthenticationHeader
    {
        /// <summary>
        /// The name of the HTTP header
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The value of the HTTP header
        /// </summary>
        public List<string> Value { get; set; }
    }
}
