using Nexus.Link.Services.Contracts.Capabilities.Integration.AppSupport;
using Nexus.Link.Services.Contracts.Capabilities.Integration.Authentication;
using Nexus.Link.Services.Contracts.Capabilities.Integration.BusinessEvents;

namespace Nexus.Link.Services.Contracts.Capabilities.Integration
{
    /// <summary>
    /// The services 
    /// </summary>
    public interface IIntegrationCapability : Libraries.Core.Platform.Services.IControllerInjector
    {
        /// <summary>
        /// Service for business events
        /// </summary>
        IBusinessEventsCapability BusinessEvents{ get; }

        /// <summary>
        /// Service for authentication
        /// </summary>
        IAuthenticationCapability Authentication{ get; }

        /// <summary>
        /// Service for centralized logging
        /// </summary>
        IAppSupportCapability AppSupport{ get; }
    }
}
