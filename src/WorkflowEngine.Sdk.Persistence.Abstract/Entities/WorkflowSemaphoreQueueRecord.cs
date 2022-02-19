using System;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.EntityAttributes;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Temporary;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities
{
    /// <summary>
    /// Support for limited concurrency
    /// </summary>
    public class WorkflowSemaphoreQueueRecord : WorkflowSemaphoreQueueRecordCreate, ICompleteTableItem, IValidatable
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
    public class WorkflowSemaphoreQueueRecordCreate : WorkflowSemaphoreQueueRecordUnique
    {
    }

    /// <summary>
    /// Support for limited concurrency
    /// </summary>
    public class WorkflowSemaphoreQueueRecordUnique : WorkflowSemaphoreQueueRecordSearch
    {
        /// <summary>
        /// The workflow instance that is waiting for semaphore <see cref="WorkflowSemaphoreQueueRecordSearch.WorkflowSemaphoreId"/>
        /// </summary>
        [Validation.NotDefault]
        public Guid WorkflowInstanceId { get; set; }

        /// <inheritdoc />
        public override string ToString() => $"{WorkflowInstanceId} waiting for semaphore {WorkflowSemaphoreId}";
    }

    /// <summary>
    /// Support for limited concurrency
    /// </summary>
    public class WorkflowSemaphoreQueueRecordSearch
    {
        /// <summary>
        /// The semaphore that we are waiting for
        /// </summary>
        [Validation.NotDefault]
        public Guid WorkflowSemaphoreId { get; set; }

        /// <inheritdoc />
        public override string ToString() => $"{WorkflowSemaphoreId}";
    }
}