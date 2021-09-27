using System;
using Nexus.Link.Libraries.Core.Assert;
using WorkflowEngine.Persistence.Abstract.Temporary;

namespace WorkflowEngine.Persistence.Abstract.Entities
{
    public class WorkflowFormRecord : WorkflowFormRecordCreate, ICompleteTableItem
    {
        /// <inheritdoc />
        public Guid Id { get; set; }

        /// <inheritdoc />
        public string Etag { get; set; }

        /// <inheritdoc />
        public DateTimeOffset RecordCreatedAt { get; set; }

        /// <inheritdoc />
        public DateTimeOffset RecordUpdatedAt { get; set; }

        /// <inheritdoc />
        public byte[] RecordVersion { get; set; }

        /// <inheritdoc />
        public override void Validate(string errorLocation, string propertyPath = "")
        {
            base.Validate(errorLocation, propertyPath);
            TableItemHelper.Validate(this, false, errorLocation, propertyPath);
        }
    }

    /// <summary>
    /// Information about a workflow.
    /// </summary>
    /// <remarks>
    /// <see cref="CapabilityName"/> and <see cref="Title"/> in combination must be unique.
    /// </remarks>
    public class WorkflowFormRecordCreate : IValidatable
    {
        public string CapabilityName { get; set; }
        public string Title { get; set; }

        /// <inheritdoc />
        public virtual void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsNotNullOrWhiteSpace(CapabilityName, nameof(CapabilityName), errorLocation);
            FulcrumValidate.IsNotNullOrWhiteSpace(Title, nameof(Title), errorLocation);
        }
    }
}