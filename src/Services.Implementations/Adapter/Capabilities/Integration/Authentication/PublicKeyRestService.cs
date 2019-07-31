using System.Threading;
using System.Threading.Tasks;
using Microsoft.Rest;
using Nexus.Link.Services.Contracts.Capabilities.Integration.Authentication;

namespace Nexus.Link.Services.Implementations.Adapter.Capabilities.Integration.Authentication
{
    /// <inheritdoc cref="IPublicKeyService" />
    public class PublicKeyRestService : RestClientBase, IPublicKeyService
    {
        public PublicKeyRestService(string baseUrl)
        :base($"{baseUrl}/PublicKeys", null)
        {
        }

        /// <inheritdoc />
        public Task<string> GetPublicRsaKeyAsXmlAsync(CancellationToken token = default(CancellationToken))
        {
            return RestClient.GetAsync<string>("", null, token);
        }
    }
}