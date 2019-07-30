using Microsoft.Rest;
using Nexus.Link.Services.Contracts.Capabilities.Integration.Authentication;

namespace Nexus.Link.Services.Implementations.Adapter.Capabilities.Integration.Authentication
{
    /// <inheritdoc />
    public class AuthenticationCapability : IAuthenticationCapability
    {
        public AuthenticationCapability(string baseUrl, ServiceClientCredentials credentials)
        {
            TokenService = new TokenRestService(baseUrl, credentials);
            PublicKeyService = new PublicKeyRestService(baseUrl, credentials);
        }

        /// <inheritdoc />
        public ITokenService TokenService { get; }

        /// <inheritdoc />
        public IPublicKeyService PublicKeyService { get; set; }
    }
}