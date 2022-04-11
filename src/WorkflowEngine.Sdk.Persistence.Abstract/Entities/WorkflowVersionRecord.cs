using System;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Storage.Logic;
using Nexus.Link.Libraries.Core.Storage.Model;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities
{
    /// <summary>
    /// Information about a specific version of a <see cref="WorkflowFormRecord"/>.
    /// </summary>
    public class WorkflowVersionRecord : WorkflowVersionRecordCreate, ICompleteTableItem
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
    public class WorkflowVersionRecordCreate : WorkflowVersionRecordUnique, IValidatable
    {
        public int MinorVersion { get; set; }
        public bool DynamicCreate { get; set; }

        /// <inheritdoc />
        public virtual void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsGreaterThanOrEqualTo(1, MajorVersion, nameof(MajorVersion), errorLocation);
            FulcrumValidate.IsGreaterThanOrEqualTo(0, MinorVersion, nameof(MinorVersion), errorLocation);
            FulcrumValidate.IsNotDefaultValue(WorkflowFormId, nameof(WorkflowFormId), errorLocation);
        }
    }

    public class WorkflowVersionRecordUnique
    {
        public Guid WorkflowFormId { get; set; }
        public int MajorVersion { get; set; }
    }
}