using Nexus.Link.Libraries.Web.RestClientHelper;
using Nexus.Link.Services.Contracts.Capabilities.Integration.Authentication;

namespace Nexus.Link.Services.Implementations.Adapter.Capabilities.Integration.Authentication
{
    /// <inheritdoc />
    public class AuthenticationCapability : IAuthenticationCapability
    {
        
        /// <summary>
        /// Constructor
        /// </summary>
        public AuthenticationCapability(IHttpSender httpSender)
        {
            TokenService = new TokenRestService(httpSender.CreateHttpSender("Tokens"));
        }

        /// <inheritdoc />
        public ITokenService TokenService { get; }
    }
}