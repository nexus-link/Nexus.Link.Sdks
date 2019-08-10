using Microsoft.Rest;
using Nexus.Link.Authentication.Sdk;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Services.Contracts.Capabilities.Integration.Authentication;

namespace Nexus.Link.Services.Implementations.BusinessApi.Capabilities.Integration.Authentication
{
    /// <inheritdoc />
    public class AuthenticationCapability : IAuthenticationCapability
    {
        /// <inheritdoc />
        public AuthenticationCapability(string serviceBaseUrl, ServiceClientCredentials serviceClientCredentials)
        {
            InternalContract.RequireNotNullOrWhiteSpace(serviceBaseUrl, nameof(serviceBaseUrl));
            InternalContract.RequireNotNull(serviceClientCredentials, nameof(serviceClientCredentials));
            var authenticationManager = new AuthenticationManager(FulcrumApplication.Setup.Tenant, serviceBaseUrl);
            TokenService = new TokenLogic(authenticationManager);
        }

        /// <inheritdoc />
        public ITokenService TokenService { get; }
    }
}
