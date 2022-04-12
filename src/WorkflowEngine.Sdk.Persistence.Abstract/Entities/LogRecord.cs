using System;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Storage.Logic;
using Nexus.Link.Libraries.Core.Storage.Model;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities
{
    public class LogRecord : LogRecordCreate, ICompleteTableItem
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

    public class LogRecordCreate : IValidatable
    {
        public Guid WorkflowFormId { get; set; }
        public Guid? WorkflowInstanceId { get; set; }
        public Guid? ActivityFormId { get; set; }

        public string SeverityLevel { get; set; }

        public int SeverityLevelNumber { get; set; }

        public string Message { get; set; }

        public string DataAsJson { get; set; }

        public DateTimeOffset TimeStamp { get; set; }

        /// <inheritdoc />
        public virtual void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsNotDefaultValue(WorkflowFormId, nameof(WorkflowFormId), errorLocation);
            if (WorkflowInstanceId.HasValue)
            {
                FulcrumValidate.IsNotDefaultValue(WorkflowInstanceId.Value, nameof(WorkflowInstanceId), errorLocation);
            }
            if (ActivityFormId.HasValue)
            {
                FulcrumValidate.IsNotDefaultValue(ActivityFormId.Value, nameof(ActivityFormId), errorLocation);
            }
            FulcrumValidate.IsNotNullOrWhiteSpace(Message, nameof(Message), errorLocation);
            FulcrumValidate.IsNotNullOrWhiteSpace(SeverityLevel, nameof(SeverityLevel), errorLocation);
        }
    }
}