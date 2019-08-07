using System.Net.Http;
using Microsoft.Rest;
using Nexus.Link.Libraries.Web.RestClientHelper;

namespace Nexus.Link.Services.Implementations.Adapter.Capabilities.Integration
{
    /// <summary>
    /// A base class for rest clients
    /// </summary>
    public abstract class RestClientBase
    {
        /// <summary>
        /// The current RestClient
        /// </summary>
        protected RestClient RestClient { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        protected RestClientBase(string baseUrl, HttpClient httpClient, ServiceClientCredentials credentials)
        {
            RestClient = new RestClient(baseUrl, httpClient, credentials);
        }
    }
}
