namespace Nexus.Link.Services.Contracts.Capabilities.Integration.BusinessEvents
{
    /// <summary>
    /// The services for business events
    /// </summary>
    public interface IBusinessEventsCapability
    {
        /// <summary>
        /// Service for business events
        /// </summary>
        IBusinessEventService BusinessEventService { get; }
    }
}
