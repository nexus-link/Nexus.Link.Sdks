namespace Nexus.Link.Services.Contracts.Capabilities.Integration.BusinessEvents.Model
{
    public class PublishableEvent : IPublishableEvent
    {
        /// <inheritdoc />
        public EventMetadata Metadata { get; set; }
    }
}