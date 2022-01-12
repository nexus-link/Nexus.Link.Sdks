using Nexus.Link.Libraries.Core.Assert;

namespace Nexus.Link.Services.Contracts.Events
{
    /// <inheritdoc />
    public class PublishableEvent : IPublishableEvent
    {
        /// <inheritdoc />
        public EventMetadata Metadata { get; set; }

        /// <inheritdoc />
        public void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsNotNull(Metadata, nameof(Metadata), errorLocation);
            FulcrumValidate.IsValidated(Metadata, $"{propertyPath}.{nameof(Metadata)}", nameof(Metadata), errorLocation);
        }
    }
}