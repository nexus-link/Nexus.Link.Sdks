﻿using Nexus.Link.Libraries.Web.RestClientHelper;
using Nexus.Link.Services.Contracts.Capabilities.Integration.BusinessEvents;

namespace Nexus.Link.Services.Implementations.Adapter.Capabilities.Integration.BusinessEvents
{
    /// <inheritdoc />
    public class BusinessEventsCapability : IBusinessEventsCapability
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public BusinessEventsCapability(IHttpSender httpSender)
        {
            BusinessEventService = new BusinessEventRestService(httpSender.CreateHttpSender("Events"));
        }

        /// <inheritdoc />
        public IBusinessEventService BusinessEventService { get; }
    }
}