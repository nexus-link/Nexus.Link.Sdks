using System;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Temporary;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities
{
    public class ActivityInstanceRecord : ActivityInstanceRecordCreate, ICompleteTableItem
    {
        public Guid Id { get; set; }
        public string Etag { get; set; }

        /// <inheritdoc />
        public DateTimeOffset RecordCreatedAt { get; set; }

        /// <inheritdoc />
        public DateTimeOffset RecordUpdatedAt { get; set; }

        /// <inheritdoc />
        public byte[] RecordVersion { get; set; }

        public DateTimeOffset? FinishedAt { get; set; }

        public bool HasCompleted { get; set; }

        public string ResultAsJson { get; set; }

        public string ExceptionName { get; set; }

        public string ExceptionMessage { get; set; }

        public string AsyncRequestId { get; set; }

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

    public class ActivityInstanceRecordCreate : IValidatable
    {
        public Guid WorkflowInstanceId { get; set; }
        public Guid ActivityVersionId { get; set; }
        public Guid? ParentActivityInstanceId { get; set; }

        public int? Iteration { get; set; }

        public DateTimeOffset StartedAt { get; set; }

        /// <inheritdoc />
        public virtual void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsNotDefaultValue(WorkflowInstanceId, nameof(WorkflowInstanceId), errorLocation);
            FulcrumValidate.IsNotDefaultValue(ActivityVersionId, nameof(ActivityVersionId), errorLocation);
            FulcrumValidate.IsLessThanOrEqualTo(DateTimeOffset.Now, StartedAt, nameof(StartedAt), errorLocation);
            if (Iteration.HasValue) FulcrumValidate.IsGreaterThanOrEqualTo(1, Iteration.Value, nameof(Iteration), errorLocation);
        }
    }
}