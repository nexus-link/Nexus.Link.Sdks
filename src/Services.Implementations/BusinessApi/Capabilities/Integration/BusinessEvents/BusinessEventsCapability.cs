using Microsoft.Rest;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Services.Contracts.Capabilities.Integration.BusinessEvents;

namespace Nexus.Link.Services.Implementations.BusinessApi.Capabilities.Integration.BusinessEvents
{
    /// <inheritdoc />
    public class BusinessEventsCapability : IBusinessEventsCapability
    {
        /// <summary>
        /// Constructor
        /// </summary>
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
