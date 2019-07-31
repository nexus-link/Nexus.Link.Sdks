using Microsoft.Rest;
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
