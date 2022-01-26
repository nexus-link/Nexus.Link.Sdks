using System;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Web.ServiceAuthentication;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Temporary;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities
{
    public class ActivityFormRecord : ActivityFormRecordCreate, ICompleteTableItem
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

    public class ActivityFormRecordCreate : IValidatable
    {
        public Guid WorkflowFormId { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }

        /// <inheritdoc />
        public virtual void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsNotDefaultValue(WorkflowFormId, nameof(WorkflowFormId), errorLocation);
            FulcrumValidate.IsNotNullOrWhiteSpace(Title, nameof(Title), errorLocation);
            FulcrumValidate.IsNotNullOrWhiteSpace(Type, nameof(Type), errorLocation);
            FulcrumValidate.IsInEnumeration(typeof(ActivityTypeEnum), Type, nameof(Type), errorLocation);
        }
    }

    public class ActivityFormRecordSearch
    {
        public Guid WorkflowFormId { get; set; }
    }
}