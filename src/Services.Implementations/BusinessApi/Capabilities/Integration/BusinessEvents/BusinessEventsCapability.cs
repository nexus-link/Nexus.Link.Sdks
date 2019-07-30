using Microsoft.Rest;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Services.Contracts.Capabilities.Integration.BusinessEvents;

namespace Nexus.Link.Services.Implementations.BusinessApi.Capabilities.Integration.BusinessEvents
{
    public class BusinessEventsCapability : IBusinessEventsCapability
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="serviceBaseUrle base URL to the business events service</param>
        /// <param name="serviceClientCredentials"></param>
        public BusinessEventsCapability(string serviceBaseUrl, ServiceClientCredentials serviceClientCredentials)
        {
            InternalContract.RequireNotNullOrWhiteSpace(serviceBaseUrl, nameof(serviceBaseUrl));
            InternalContract.RequireNotNull(serviceClientCredentials, nameof(serviceClientCredentials));
            BusinessEventService = new BusinessEventLogic(serviceBaseUrl, serviceClientCredentials);
        }

        /// <inheritdoc />
        public IBusinessEventService BusinessEventService { get; }
    }
}
