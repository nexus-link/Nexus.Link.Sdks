using Microsoft.Rest;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Web.Platform.Authentication;
using Nexus.Link.Services.Contracts.Capabilities.Integration.BusinessEvents;

namespace Nexus.Link.Services.Implementations.Integration.BusinessEvents
{
    public class BusinessEventsCapability : IBusinessEventsCapability
    {
        public BusinessEventsCapability(ServiceClientCredentials serviceClientCredentials)
        {
            var serviceBaseUrl = FulcrumApplication.AppSettings.GetString("NexusBusinessEventsUrl", true);
            BusinessEventService = new BusinessEventLogic(serviceBaseUrl, serviceClientCredentials);
        }

        /// <inheritdoc />
        public IBusinessEventService BusinessEventService { get; }
    }
}
