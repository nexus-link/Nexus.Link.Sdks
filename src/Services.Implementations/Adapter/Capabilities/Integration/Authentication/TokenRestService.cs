using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Platform.Authentication;
using Nexus.Link.Services.Contracts.Capabilities.Integration.Authentication;

namespace Nexus.Link.Services.Implementations.Adapter.Capabilities.Integration.Authentication
{
    /// <inheritdoc cref="ITokenService" />
    public class TokenRestService : RestClientBase, ITokenService
    {
        /// <inheritdoc cref="ITokenService" />
        public TokenRestService(string baseUrl, HttpClient httpClient)
        :base($"{baseUrl}/Tokens", httpClient, null)
        {
        }

        /// <inheritdoc />
        public Task<AuthenticationToken> ObtainAccessTokenAsync(AuthenticationCredentials credentials, CancellationToken token = default(CancellationToken))
        {
            return RestClient.PostAsync<AuthenticationToken, AuthenticationCredentials>("", credentials, null, token);
        }
    }
}