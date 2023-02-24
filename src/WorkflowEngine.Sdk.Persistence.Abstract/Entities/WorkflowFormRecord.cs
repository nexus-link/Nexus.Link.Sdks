using System;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Storage.Logic;
using Nexus.Link.Libraries.Core.Storage.Model;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities
{
    public class WorkflowFormRecord : WorkflowFormRecordCreate, ICompleteTableItem
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
    /// Information about a workflow.
    /// </summary>
    public class WorkflowFormRecordCreate : WorkflowFormRecordUnique
    {
    }
    
    /// <summary>
    /// Information about a workflow.
    /// </summary>
    public class WorkflowFormRecordOverview : WorkflowForm
    {
        /// <summary>
        /// Overview of instance counts
        /// </summary>
        public WorkflowFormInstancesOverview Overview { get; set; }
        /// <summary>
        /// Version
        /// </summary>
        public string Version { get; set; }
    }

    /// <summary>
    /// Information about a workflow.
    /// </summary>
    public class WorkflowFormRecordUnique : IValidatable
    {
        public string CapabilityName { get; set; }
        public string Title { get; set; }

        /// <inheritdoc />
        public virtual void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsNotNullOrWhiteSpace(CapabilityName, nameof(CapabilityName), errorLocation);
            FulcrumValidate.IsNotNullOrWhiteSpace(Title, nameof(Title), errorLocation);
        }
    }
}