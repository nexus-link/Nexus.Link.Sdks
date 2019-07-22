using Microsoft.Rest;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Web.Platform.Authentication;
using Nexus.Link.Services.Contracts.Capabilities.Integration.Authentication;

namespace Nexus.Link.Services.Implementations.Integration.Authentication
{
    public class AuthenticationCapability : IAuthenticationCapability
    {
        public AuthenticationCapability(ServiceClientCredentials serviceClientCredentials)
        {
            var serviceBaseUrl = FulcrumApplication.AppSettings.GetString("NexusAuthenticationAsAServiceUrl", true);
            TokenService = new TokenLogic(serviceBaseUrl);
        }

        /// <inheritdoc />
        public ITokenService TokenService { get; }
    }
}
