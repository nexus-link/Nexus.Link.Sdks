using System.Net.Http;
using Microsoft.Rest;
using Nexus.Link.Libraries.Web.Pipe.Outbound;
using Nexus.Link.Libraries.Web.RestClientHelper;

namespace Nexus.Link.Services.Implementations.Adapter.Capabilities.Integration
{
    public abstract class RestClientBase
    {
        protected static HttpClient _httpClient;
        protected static readonly object ClassLock = new object();
        protected readonly RestClient _restClient;

        public RestClientBase(string baseUrl, ServiceClientCredentials credentials)
        {
            lock (ClassLock)
            {
                if (_httpClient == null)
                {
                    _httpClient = HttpClientFactory.Create(OutboundPipeFactory.CreateDelegatingHandlers());
                }
            }
            var url = $"{baseUrl}/Integration/v1/PublicKeys";
            _restClient = new RestClient(url, _httpClient, credentials);
        }
    }
}
