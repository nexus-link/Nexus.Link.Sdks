using System;
using Nexus.Link.Libraries.Core.Assert;
using WorkflowEngine.Persistence.Abstract.Temporary;

namespace WorkflowEngine.Persistence.Abstract.Entities
{
    /// <summary>
    /// Information about a specific version of a <see cref="WorkflowFormRecord"/>.
    /// </summary>
    public class WorkflowInstanceRecord : WorkflowInstanceRecordCreate, ICompleteTableItem
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

        public DateTimeOffset? FinishedAt { get; set; }

        /// <inheritdoc />
        public override void Validate(string errorLocation, string propertyPath = "")
        {
            base.Validate(errorLocation, propertyPath);
            TableItemHelper.Validate(this, false, errorLocation, propertyPath);
            if (FinishedAt != null)
            {
                FulcrumValidate.IsLessThanOrEqualTo(DateTimeOffset.Now, FinishedAt.Value, nameof(FinishedAt), errorLocation);
                FulcrumValidate.IsGreaterThanOrEqualTo(StartedAt, FinishedAt.Value, nameof(FinishedAt), errorLocation);
            }
        }
    }

    /// <summary>
    /// Information about a specific version of a <see cref="WorkflowFormRecord"/>.
    /// </summary>
    public class WorkflowInstanceRecordCreate : IValidatable
    {
        public Guid WorkflowVersionId { get; set; }

        public string Title { get; set; }

        public string InitialVersion { get; set; }

        public DateTimeOffset StartedAt { get; set; }

        /// <inheritdoc />
        public virtual void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsNotNullOrWhiteSpace(Title, nameof(Title), errorLocation);
            FulcrumValidate.IsNotNullOrWhiteSpace(InitialVersion, nameof(InitialVersion), errorLocation);
            FulcrumValidate.IsLessThanOrEqualTo(DateTimeOffset.Now, StartedAt, nameof(StartedAt), errorLocation);
        }
    }
}