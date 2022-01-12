using System;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Platform.DataSyncEngine;

namespace Nexus.Link.Services.Contracts.Events.SynchronizedEntity
{
    /// <summary>
    /// This event is published whenever a new Invoices has been published.
    /// </summary>
    public class DataSyncEntityWasUpdated : IPublishableEvent
    {
        /// <inheritdoc />
        public EventMetadata Metadata { get; set; } = 
            new EventMetadata("DataSyncEntity", "Updated", 1, 0);
        
        /// <summary>
        /// The entity key that the update was for
        /// </summary>
        public SyncKey Key { get; } = new SyncKey();

        /// <summary>
        /// The time (ISO8061) when the object was updated.
        /// </summary>
        public string Timestamp { get; } = DateTime.UtcNow.ToString("o");

        /// <summary>
        /// Optional. Name of the user that caused the update.
        /// </summary>
        public string UserName { get; set; }

        /// <inheritdoc />
        public void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsNotNull(Metadata, nameof(Metadata), errorLocation);
            FulcrumValidate.IsValidated(Metadata, $"{propertyPath}.{nameof(Metadata)}", nameof(Metadata), errorLocation);
            FulcrumValidate.IsNotNull(Key, nameof(Key), errorLocation);
            FulcrumValidate.IsValidated(Key, $"{propertyPath}.{nameof(Key)}", nameof(Key), errorLocation);
        }
    }
}
