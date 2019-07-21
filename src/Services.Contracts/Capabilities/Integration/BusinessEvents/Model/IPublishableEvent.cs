namespace Nexus.Link.Services.Contracts.Capabilities.Integration.BusinessEvents.Model
{
    /// <summary>
    /// Minimum data for a publishable event
    /// </summary>
    public interface IPublishableEvent
    {
        EventMetadata Metadata { get; }
    }
}