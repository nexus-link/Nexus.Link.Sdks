using System;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Temporary;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities
{
    public class TransitionRecord : TransitionRecordCreate, ICompleteTableItem
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

    public class TransitionRecordCreate : TransitionRecordUnique, IValidatable
    {

        /// <inheritdoc />
        public virtual void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsNotDefaultValue(WorkflowVersionId, nameof(WorkflowVersionId), errorLocation);
            if (FromActivityVersionId == null)
            {
                FulcrumValidate.IsTrue(ToActivityVersionId != null, errorLocation, $"One of {nameof(FromActivityVersionId)} and {nameof(ToActivityVersionId)} must be not null.");
            }
        }
    }

    public class TransitionRecordUnique
    {
        public Guid WorkflowVersionId { get; set; }
        public Guid? FromActivityVersionId { get; set; }
        public Guid? ToActivityVersionId { get; set; }
    }
}