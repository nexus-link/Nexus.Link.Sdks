using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Services.Contracts.Capabilities.Integration.Authentication;

namespace Nexus.Link.Services.Implementations.Adapter.Capabilities.Integration.Authentication
{
    /// <inheritdoc cref="IPublicKeyService" />
    public class PublicKeyRestService : RestClientBase, IPublicKeyService
    {
        /// <inheritdoc cref="IPublicKeyService" />
        public PublicKeyRestService(string baseUrl, HttpClient httpClient)
        :base($"{baseUrl}/PublicKeys", httpClient, null)
        {
        }

        /// <inheritdoc />
        public Task<string> GetPublicRsaKeyAsXmlAsync(CancellationToken token = default(CancellationToken))
        {
            return RestClient.GetAsync<string>("", null, token);
        }
    }
}