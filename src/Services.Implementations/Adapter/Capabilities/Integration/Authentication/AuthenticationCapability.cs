using Microsoft.Rest;
using Nexus.Link.Services.Contracts.Capabilities.Integration.Authentication;

namespace Nexus.Link.Services.Implementations.Adapter.Capabilities.Integration.Authentication
{
    /// <inheritdoc />
    public class AuthenticationCapability : IAuthenticationCapability
    {
        public AuthenticationCapability(string baseUrl)
        {
            TokenService = new TokenRestService(baseUrl);
            PublicKeyService = new PublicKeyRestService(baseUrl);
        }

        /// <inheritdoc />
        public ITokenService TokenService { get; }

        /// <inheritdoc />
        public IPublicKeyService PublicKeyService { get; set; }
    }
}