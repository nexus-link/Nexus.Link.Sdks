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
        /// <summary>
        /// When does this semaphore expire. Only set if <see cref="Raised"/> == true
        /// </summary>
        [Validation.NotNull(TriggerPropertyName = nameof(Raised))]
        public DateTimeOffset? ExpiresAt { get; set; }

        /// <summary>
        /// After how many seconds should this semaphore expire (from the time it was raised or extended).
        /// </summary>
        public double ExpirationAfterSeconds { get; set; }

        /// <summary>
        /// True if the semaphore is raised for this workflow instance, false if it is in queue.
        /// </summary>
        public bool Raised { get; set; }
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

        /// <summary>
        /// The activity instance that is waiting for semaphore <see cref="WorkflowSemaphoreQueueRecordSearch.WorkflowSemaphoreId"/>
        /// </summary>
        public Guid? ParentActivityInstanceId { get; set; }

        /// <summary>
        /// The parent iteration for the activity that raised this semaphore, or null if no parent exists.
        /// </summary>
        public int? ParentIteration { get; set; }

        /// <inheritdoc />
        public override string ToString() => $"{WorkflowSemaphoreId} for workflow {WorkflowInstanceId} and activity {ParentActivityInstanceId}";
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