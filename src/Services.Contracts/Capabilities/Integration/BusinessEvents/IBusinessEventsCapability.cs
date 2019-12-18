namespace Nexus.Link.Services.Contracts.Capabilities.Integration.BusinessEvents
{
    /// <summary>
    /// Business Events
    /// </summary>
    public interface IBusinessEventsCapability : IControllerInjector
    {
        /// <summary>
        /// Service for business events
        /// </summary>
        IBusinessEventService BusinessEventService { get; }
    }
}
