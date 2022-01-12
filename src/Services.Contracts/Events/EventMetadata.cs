using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Logging;

namespace Nexus.Link.Services.Contracts.Events
{
    /// <summary>
    /// Metadata for an event
    /// </summary>
    public class EventMetadata : IValidatable, ILoggable
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public EventMetadata()
        {

        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="entityName">The entity name, e.g. "User"</param>
        /// <param name="eventName">The event name, e.g. LoggedIn</param>
        /// <param name="majorVersion">The major version for the event schema.</param>
        /// <param name="minorVersion">The minor version for the event schema.</param>
        public EventMetadata(string entityName, string eventName, int majorVersion, int minorVersion)
        {
            EntityName = entityName;
            EventName = eventName;
            MajorVersion = majorVersion;
            MinorVersion = minorVersion;
        }

        /// <summary>
        /// The name of the entity, e.g. "Invoice"
        /// </summary>
        public string EntityName{ get; set; }

        /// <summary>
        /// The name of the event, e.g. "PaidInFull"
        /// </summary>
        public string EventName { get; set; }

        /// <summary>
        /// The major version for this event. Only introduce new versions when there is a breaking change, e.g. renaming or removal of fields.
        /// </summary>
        public int MajorVersion { get; set; }

        /// <summary>
        /// The minor version for this event. Bump this whenever the event contract is updated. See also <see cref="MajorVersion"/>.
        /// </summary>
        public int MinorVersion { get; set; }

        /// <inheritdoc />
        public void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsNotNullOrWhiteSpace(EntityName, nameof(EntityName), errorLocation);
            FulcrumValidate.IsNotNullOrWhiteSpace(EventName, nameof(EventName), errorLocation);
            FulcrumValidate.IsGreaterThanOrEqualTo(0, MajorVersion, nameof(MajorVersion), errorLocation);
            FulcrumValidate.IsGreaterThanOrEqualTo(0, MinorVersion, nameof(MinorVersion), errorLocation);
        }

        /// <summary>
        /// Implements <see cref="ILoggable"/>
        /// </summary>
        public string ToLogString() => $"{EntityName}.{EventName} ({MajorVersion}.{MinorVersion})";

        /// <inheritdoc />
        public override string ToString() => ToLogString();
    }
}
