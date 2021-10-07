using System;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Temporary;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities
{
    public class MethodParameterRecord : MethodParameterRecordCreate, ICompleteTableItem
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

    public class MethodParameterRecordCreate : MethodParameterRecordUnique, IValidatable
    {
        /// <inheritdoc />
        public virtual void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsNotDefaultValue(XVersionId, nameof(XVersionId), errorLocation);
            FulcrumValidate.IsNotNullOrWhiteSpace(Name, nameof(Name), errorLocation);
        }
    }

    public class MethodParameterRecordUnique
    {
        public Guid XVersionId { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }
    }
}