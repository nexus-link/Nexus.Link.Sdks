using System;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Temporary;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities
{
    public class ActivityVersionRecord : ActivityVersionRecordCreate, ICompleteTableItem
    {
        public Guid Id { get; set; }
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

    public class ActivityVersionRecordCreate : IValidatable
    {
        public Guid WorkflowVersionId { get; set; }
        public Guid ActivityFormId { get; set; }
        public int Position { get; set; }
        public Guid? ParentActivityVersionId { get; set; }
        public string FailUrgency { get; set; }

        /// <inheritdoc />
        public virtual void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsGreaterThanOrEqualTo(1, Position, nameof(Position), errorLocation);
            FulcrumValidate.IsNotDefaultValue(WorkflowVersionId, nameof(WorkflowVersionId), errorLocation);
            FulcrumValidate.IsNotDefaultValue(ActivityFormId, nameof(ActivityFormId), errorLocation);
            FulcrumValidate.IsNotNullOrWhiteSpace(FailUrgency, nameof(FailUrgency), errorLocation);
        }
    }

    public class ActivityVersionRecordSearch
    {
        public Guid WorkflowVersionId { get; set; }
    }
}