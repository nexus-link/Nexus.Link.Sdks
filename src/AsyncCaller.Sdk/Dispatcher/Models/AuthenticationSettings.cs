using System.Collections.Generic;
using Nexus.Link.Libraries.Web.ServiceAuthentication;

namespace Nexus.Link.AsyncCaller.Sdk.Dispatcher.Models
{
    /// <summary>
    /// Settings to support refreshing authentication credentials.
    /// </summary>
    public class AuthenticationSettings
    {
        /// <summary>
        /// A request to TokenUrl of an originator requires authentication and one of the Methods will be used to that.
        /// </summary>
        public List<ClientAuthorizationSettings> Methods { get; set; }

        /// <summary>
        /// When a request's authenticaton needs renewal, Nexus Async Caller looks for the orginating client in this list.
        /// If it is found, the AuthenticationMethod is used to get authentication
        /// and then a POST request is made to TokenUrl to get new credentials.
        /// </summary>
        public List<Originator> Originators { get; set; }
    }

    /// <summary>
    /// A client that initiated a request to Async Caller
    /// </summary>
    public class Originator
    {
        /// <summary>
        /// The name of the originating client.
        ///
        /// Note! Must match the "unique_name" claim in the JWT used to make the initial request.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// A reference to the <see cref="AuthenticationMethod"/> to use to fetching credentials to call <see cref="TokenUrl"/>
        /// </summary>
        public string AuthenticationMethod { get; set; }

        /// <summary>
        /// The originator's endpoint from where to rewnew the authentication credentials.
        /// </summary>
        public string TokenUrl { get; set; }
    }
}