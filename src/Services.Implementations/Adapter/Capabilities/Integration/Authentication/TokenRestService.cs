using System.Threading;
using System.Threading.Tasks;
using Microsoft.Rest;
using Nexus.Link.Libraries.Core.Platform.Authentication;
using Nexus.Link.Services.Contracts.Capabilities.Integration.Authentication;

namespace Nexus.Link.Services.Implementations.Adapter.Capabilities.Integration.Authentication
{
    /// <inheritdoc cref="ITokenService" />
    public class TokenRestService : RestClientBase, ITokenService
    {
        public TokenRestService(string baseUrl, ServiceClientCredentials credentials)
        :base(baseUrl, credentials)
        {
        }

        /// <inheritdoc />
        public Task<AuthenticationToken> ObtainAccessToken(AuthenticationCredentials credentials, CancellationToken token = new CancellationToken())
        {
            return _restClient.GetAsync<AuthenticationToken>("", null, token);
        }
    }
}