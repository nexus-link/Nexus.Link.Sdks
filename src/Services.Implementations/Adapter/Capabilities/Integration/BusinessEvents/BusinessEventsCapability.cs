using System.Net.Http;
using Microsoft.Rest;
using Nexus.Link.Services.Contracts.Capabilities.Integration.BusinessEvents;

namespace Nexus.Link.Services.Implementations.Adapter.Capabilities.Integration.BusinessEvents
{
    /// <inheritdoc />
    public class BusinessEventsCapability : IBusinessEventsCapability
    {
        /// <inheritdoc />
        public BusinessEventsCapability(string baseUrl, HttpClient httpClient, ServiceClientCredentials credentials)
        {
            BusinessEventService = new BusinessEventRestService($"{baseUrl}/Events", httpClient, credentials);
        }

        /// <inheritdoc />
        public IBusinessEventService BusinessEventService { get; }
    }
}