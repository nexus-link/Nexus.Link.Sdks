using System;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Storage.Logic;
using Nexus.Link.Libraries.Core.Storage.Model;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities
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

        /// <inheritdoc />
        public override void Validate(string errorLocation, string propertyPath = "")
        {
            base.Validate(errorLocation, propertyPath);
            TableItemHelper.Validate(this, false, errorLocation, propertyPath);
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

        public string State { get; set; }

        public DateTimeOffset? FinishedAt { get; set; }
        
        public DateTimeOffset? CancelledAt { get; set; }

        public bool IsComplete { get; set; }

        public string ResultAsJson { get; set; }

        public string ExceptionTechnicalMessage { get; set; }

        public string ExceptionFriendlyMessage { get; set; }

        /// <inheritdoc />
        public virtual void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsNotDefaultValue(WorkflowVersionId, nameof(WorkflowVersionId), errorLocation);
            FulcrumValidate.IsNotNullOrWhiteSpace(Title, nameof(Title), errorLocation);
            FulcrumValidate.IsNotNullOrWhiteSpace(InitialVersion, nameof(InitialVersion), errorLocation);
            FulcrumValidate.IsNotDefaultValue(StartedAt, nameof(StartedAt), errorLocation);
            FulcrumValidate.IsLessThanOrEqualTo(DateTimeOffset.Now, StartedAt, nameof(StartedAt), errorLocation);
            if (FinishedAt.HasValue)
            {
                FulcrumValidate.IsNotDefaultValue(FinishedAt.Value, nameof(FinishedAt), errorLocation);
                FulcrumValidate.IsLessThanOrEqualTo(DateTimeOffset.Now, FinishedAt.Value, nameof(FinishedAt), errorLocation);
                FulcrumValidate.IsGreaterThanOrEqualTo(StartedAt, FinishedAt.Value, nameof(FinishedAt), errorLocation);
            }
            if (CancelledAt.HasValue)
            {
                FulcrumValidate.IsNotDefaultValue(CancelledAt.Value, nameof(CancelledAt), errorLocation);
                FulcrumValidate.IsLessThanOrEqualTo(DateTimeOffset.Now, CancelledAt.Value, nameof(CancelledAt), errorLocation);
                FulcrumValidate.IsGreaterThanOrEqualTo(StartedAt, CancelledAt.Value, nameof(CancelledAt), errorLocation);
            }
            FulcrumValidate.IsJson(ResultAsJson, nameof(ResultAsJson), errorLocation);
        }
    }
}