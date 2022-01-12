using Nexus.Link.Libraries.Core.Assert;

namespace Nexus.Link.Services.Contracts.Events
{
    /// <summary>
    /// Minimum data for a publishable event
    /// </summary>
    public interface IPublishableEvent : IValidatable
    {
        /// <summary>
        /// Metadata for the event, e.g. name and version.
        /// </summary>
        EventMetadata Metadata { get; }
    }
}