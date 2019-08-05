using Microsoft.Rest;
using Nexus.Link.Authentication.Sdk;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Platform.Authentication;
using Nexus.Link.Libraries.Web.Platform.Authentication;
using Nexus.Link.Services.Contracts.Capabilities.Integration;
using Nexus.Link.Services.Contracts.Capabilities.Integration.Authentication;
using Nexus.Link.Services.Contracts.Capabilities.Integration.BusinessEvents;
using Nexus.Link.Services.Contracts.Capabilities.Integration.Logging;
using Nexus.Link.Services.Implementations.Adapter.Capabilities.Integration.Authentication;
using Nexus.Link.Services.Implementations.Adapter.Capabilities.Integration.BusinessEvents;
using Nexus.Link.Services.Implementations.Adapter.Capabilities.Integration.Logging;

namespace Nexus.Link.Services.Implementations.Adapter.Capabilities.Integration
{
    /// <inheritdoc />
    public class IntegrationCapability : IIntegrationCapability
    {
        private static ITokenRefresherWithServiceClient _tokenRefresher;
        private static readonly object ClassLock = new object();

        private static ITokenRefresherWithServiceClient TokenRefresher(string baseUrl,
            AuthenticationCredentials credentials)
        {

            lock (ClassLock)
            {
                if (_tokenRefresher != null) return _tokenRefresher;
                _tokenRefresher =
                    AuthenticationManager.CreateTokenRefresher(FulcrumApplication.Setup.Tenant, baseUrl, credentials);
                FulcrumAssert.IsNotNull(_tokenRefresher);
                return _tokenRefresher;
            }
        }

        private static ServiceClientCredentials ServiceClientCredentials(string baseUrl,
            AuthenticationCredentials credentials) => TokenRefresher(baseUrl, credentials).GetServiceClient();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="baseUrl">The URL to the integration capability</param>
        /// <param name="basicCredentials">ClientId and ClientSecret for calling the business api</param>
        public IntegrationCapability(string baseUrl, AuthenticationCredentials basicCredentials) : this(baseUrl, ServiceClientCredentials(baseUrl, basicCredentials))
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="baseUrl">The URL to the integration capability</param>
        /// <param name="credentials">Credentials for calls to the business api</param>
        public IntegrationCapability(string baseUrl, ServiceClientCredentials credentials)
        {
            BusinessEvents = new BusinessEventsCapability($"{baseUrl}/BusinessEvents", credentials);
            Authentication = new AuthenticationCapability($"{baseUrl}/Authentication");
            Logging = new LoggingCapability($"{baseUrl}/Logging", credentials);
        }

        /// <inheritdoc />
        public IBusinessEventsCapability BusinessEvents { get; }

        /// <inheritdoc />
        public IAuthenticationCapability Authentication { get; }

        /// <inheritdoc />
        public ILoggingCapability Logging { get; }
    }
}
