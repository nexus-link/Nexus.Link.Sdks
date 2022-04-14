using System;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.EntityAttributes;
using Nexus.Link.Libraries.Core.Storage.Logic;
using Nexus.Link.Libraries.Core.Storage.Model;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities
{
    /// <summary>
    /// Support for limited concurrency
    /// </summary>
    public class WorkflowSemaphoreRecord : WorkflowSemaphoreRecordCreate, ICompleteTableItem, IValidatable
    {
        /// <inheritdoc />
        [Hint.PrimaryKey]
        [Validation.NotDefault]
        public Guid Id { get; set; }

        /// <inheritdoc />
        [Hint.OptimisticConcurrencyControl]
        [Validation.NotNullOrWhitespace]
        public string Etag { get; set; }

        /// <inheritdoc />
        [Hint.RecordCreatedAt]
        public DateTimeOffset RecordCreatedAt { get; set; }

        /// <inheritdoc />
        [Hint.RecordUpdatedAt]
        [Validation.GreaterThanOrEqualToProperty(nameof(RecordCreatedAt))]
        public DateTimeOffset RecordUpdatedAt { get; set; }

        /// <inheritdoc />
        public byte[] RecordVersion { get; set; }

        /// <inheritdoc />
        public virtual void Validate(string errorLocation, string propertyPath = "")
        {
            TableItemHelper.Validate(this, false, errorLocation, propertyPath);
        }
    }

    /// <summary>
    /// Support for limited concurrency
    /// </summary>
    public class WorkflowSemaphoreRecordCreate : WorkflowSemaphoreRecordUnique
    {

        /// <summary>
        /// The maximum number of concurrent holders for this semaphore
        /// </summary>
        [Validation.GreaterThanOrEqualTo(1)]
        public int Limit { get; set; }
    }

    /// <summary>
    /// Support for limited concurrency
    /// </summary>
    public class WorkflowSemaphoreRecordUnique
    {
        /// <summary>
        /// The workflow that this semaphore is for
        /// </summary>
        [Validation.NotDefault]
        public Guid WorkflowFormId { get; set; }

        /// <summary>
        /// This value is used to distinguish different semaphores for the same workflow
        /// </summary>
        [Validation.NotNull]
        public string ResourceIdentifier { get; set; }

        /// <inheritdoc />
        public override string ToString() => $"{ResourceIdentifier} for workflow form {WorkflowFormId}";
    }
}