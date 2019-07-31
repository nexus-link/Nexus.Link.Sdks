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
        public TokenRestService(string baseUrl)
        :base($"{baseUrl}/Tokens", null)
        {
        }

        /// <inheritdoc />
        public Task<AuthenticationToken> ObtainAccessTokenAsync(AuthenticationCredentials credentials, CancellationToken token = default(CancellationToken))
        {
            return RestClient.PostAsync<AuthenticationToken, AuthenticationCredentials>("", credentials, null, token);
        }
    }
}