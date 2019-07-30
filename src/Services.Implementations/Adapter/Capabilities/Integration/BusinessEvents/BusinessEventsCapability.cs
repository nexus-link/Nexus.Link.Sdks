using Microsoft.Rest;
using Nexus.Link.Services.Contracts.Capabilities.Integration.BusinessEvents;

namespace Nexus.Link.Services.Implementations.Adapter.Capabilities.Integration.BusinessEvents
{
    /// <inheritdoc />
    public class BusinessEventsCapability : IBusinessEventsCapability
    {
        public BusinessEventsCapability(string baseUrl, ServiceClientCredentials credentials)
        {
            BusinessEventService = new BusinessEventRestService(baseUrl, credentials);
        }

        /// <inheritdoc />
        public IBusinessEventService BusinessEventService { get; }
    }
}