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

        public string EntityName{ get; set; }
        public string EventName { get; set; }
        public int MajorVersion { get; set; }
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

        public override string ToString() => ToLogString();
    }
}
