using System.Net.Http;
using Microsoft.Rest;
using Nexus.Link.Libraries.Web.Pipe.Outbound;
using Nexus.Link.Libraries.Web.RestClientHelper;

namespace Nexus.Link.Services.Implementations.Adapter.Capabilities.Integration
{
    public abstract class RestClientBase
    {
        protected static HttpClient HttpClient { get; private set; }
        protected static readonly object ClassLock = new object();
        protected RestClient RestClient { get; }

        protected RestClientBase(string baseUrl, ServiceClientCredentials credentials)
        {
            lock (ClassLock)
            {
                if (HttpClient == null)
                {
                    HttpClient = HttpClientFactory.Create(OutboundPipeFactory.CreateDelegatingHandlers());
                }
            }
            RestClient = new RestClient(baseUrl, HttpClient, credentials);
        }
    }
}
