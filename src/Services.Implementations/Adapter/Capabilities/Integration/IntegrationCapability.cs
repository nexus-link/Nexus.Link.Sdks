using System.Net.Http;
using Microsoft.Rest;
using Nexus.Link.Authentication.Sdk.Logic;
using Nexus.Link.Libraries.Core.Platform.Authentication;
using Nexus.Link.Libraries.Web.Pipe.Outbound;
using Nexus.Link.Libraries.Web.Platform.Authentication;
using Nexus.Link.Services.Contracts.Capabilities.Integration;
using Nexus.Link.Services.Contracts.Capabilities.Integration.AppSupport;
using Nexus.Link.Services.Contracts.Capabilities.Integration.Authentication;
using Nexus.Link.Services.Contracts.Capabilities.Integration.BusinessEvents;
using Nexus.Link.Services.Implementations.Adapter.Capabilities.Integration.AppSupport;
using Nexus.Link.Services.Implementations.Adapter.Capabilities.Integration.Authentication;
using Nexus.Link.Services.Implementations.Adapter.Capabilities.Integration.BusinessEvents;

namespace Nexus.Link.Services.Implementations.Adapter.Capabilities.Integration
{
    /// <inheritdoc />
    public class IntegrationCapability : IIntegrationCapability
    {
        /// <summary>
        /// The HttpClient to use for all integration capabilities
        /// </summary>
        protected static HttpClient HttpClient { get; private set; }
        private static ITokenRefresherWithServiceClient _tokenRefresher;
        private static readonly object ClassLock = new object();

        private ITokenRefresherWithServiceClient TokenRefresher(AuthenticationCredentials credentials)
        {
            lock (ClassLock)
            {
                if (_tokenRefresher != null) return _tokenRefresher;
                var authenticationManager = new AdapterAuthenticationManager(Authentication.TokenService);
                _tokenRefresher = new TokenRefresher(authenticationManager, credentials);
                return _tokenRefresher;
            }
        }

        private ServiceClientCredentials ServiceClientCredentials(string baseUrl,
            AuthenticationCredentials credentials) => TokenRefresher(credentials).GetServiceClient();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="baseUrl">The URL to the integration capability</param>
        /// <param name="basicCredentials">ClientId and ClientSecret for calling the business api</param>
        public IntegrationCapability(string baseUrl, AuthenticationCredentials basicCredentials)
        {
            lock (ClassLock)
            {
                if (HttpClient == null)
                {
                    HttpClient = HttpClientFactory.Create(OutboundPipeFactory.CreateDelegatingHandlers());
                }
            }
            Authentication = new AuthenticationCapability($"{baseUrl}/Authentication", HttpClient);
            var credentials = ServiceClientCredentials(baseUrl, basicCredentials);
            BusinessEvents = new BusinessEventsCapability($"{baseUrl}/BusinessEvents", HttpClient, credentials);
            AppSupport = new AppSupportCapability($"{baseUrl}/AppSupport", HttpClient, credentials);
        }

        /// <inheritdoc />
        public IBusinessEventsCapability BusinessEvents { get; }

        /// <inheritdoc />
        public IAuthenticationCapability Authentication { get; }

        /// <inheritdoc />
        public IAppSupportCapability AppSupport { get; }
    }
}
