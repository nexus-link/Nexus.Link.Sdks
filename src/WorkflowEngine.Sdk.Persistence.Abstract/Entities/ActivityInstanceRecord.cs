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

        public string ExceptionCategory { get; set; }

        public string ExceptionTechnicalMessage { get; set; }

        public string ExceptionFriendlyMessage { get; set; }

        public string AsyncRequestId { get; set; }

        public bool? ExceptionAlertHandled { get; set; }

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

    public class ActivityInstanceRecordCreate : ActivityInstanceRecordUnique, IValidatable
    {
        public DateTimeOffset StartedAt { get; set; }
        public string State { get; set; }

        /// <inheritdoc />
        public virtual void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsNotDefaultValue(WorkflowInstanceId, nameof(WorkflowInstanceId), errorLocation);
            FulcrumValidate.IsNotDefaultValue(ActivityVersionId, nameof(ActivityVersionId), errorLocation);
            FulcrumValidate.IsLessThanOrEqualTo(DateTimeOffset.Now, StartedAt, nameof(StartedAt), errorLocation);
            if (ParentIteration.HasValue) FulcrumValidate.IsGreaterThanOrEqualTo(1, ParentIteration.Value, nameof(ParentIteration), errorLocation);
        }
    }

    public class ActivityInstanceRecordUnique
    {
        public Guid WorkflowInstanceId { get; set; }
        public Guid ActivityVersionId { get; set; }
        public Guid? ParentActivityInstanceId { get; set; }

        public int? ParentIteration { get; set; }
    }

    public class ActivityInstanceRecordSearch
    {
        public Guid WorkflowInstanceId { get; set; }
    }
}