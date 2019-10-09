using System.Net.Http;
using Nexus.Link.Services.Contracts.Capabilities.Integration.Authentication;

namespace Nexus.Link.Services.Implementations.Adapter.Capabilities.Integration.Authentication
{
    /// <inheritdoc />
    public class AuthenticationCapability : IAuthenticationCapability
    {
        
        /// <inheritdoc />
        public AuthenticationCapability(string baseUrl, HttpClient httpClient)
        {
            TokenService = new TokenRestService($"{baseUrl}/Tokens", httpClient);
        }

        /// <inheritdoc />
        public ITokenService TokenService { get; }
    }
}