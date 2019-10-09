using System.Net.Http;
using Microsoft.Rest;
using Nexus.Link.Authentication.Sdk.Logic;
using Nexus.Link.Libraries.Core.Application;
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
        private static HttpClient _httpClient;
        private static ITokenRefresherWithServiceClient _tokenRefresher;
        private static readonly object ClassLock = new object();

        /// <summary>
        /// The HttpClient to use for all integration capabilities
        /// </summary>
        protected static HttpClient HttpClient
        {
            get
            {
                if (FulcrumApplication.IsInDevelopment) return null;
                lock (ClassLock)
                {
                    if (_httpClient == null)
                    {
                        _httpClient = HttpClientFactory.Create(OutboundPipeFactory.CreateDelegatingHandlers());
                    }
                }

                return _httpClient;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="baseUrl">The URL to the integration capability</param>
        /// <param name="basicCredentials">ClientId and ClientSecret for calling the business api</param>
        public IntegrationCapability(string baseUrl, AuthenticationCredentials basicCredentials)
        {
            Authentication = new AuthenticationCapability($"{baseUrl}/Authentication/v1", HttpClient);
            var credentials = ServiceClientCredentials(baseUrl, basicCredentials);
            BusinessEvents = new BusinessEventsCapability($"{baseUrl}/BusinessEvents/v1", HttpClient, credentials);
            AppSupport = new AppSupportCapability($"{baseUrl}/AppSupport/v1", HttpClient, credentials);
        }

        /// <inheritdoc />
        public IBusinessEventsCapability BusinessEvents { get; }

        /// <inheritdoc />
        public IAuthenticationCapability Authentication { get; }

        /// <inheritdoc />
        public IAppSupportCapability AppSupport { get; }

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
    }
}
