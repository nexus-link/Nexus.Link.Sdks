﻿using System;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.EntityAttributes;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Temporary;

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
        /// The workflow instance that raised this semaphore
        /// </summary>
        [Validation.NotDefault]
        public Guid WorkflowInstanceId { get; set; }

        /// <summary>
        /// If this is true, then the semaphore is raised
        /// </summary>
        public bool Raised { get; set; }

        /// <summary>
        /// When does this semaphore expire
        /// </summary>
        public DateTimeOffset ExpiresAt { get; set; }
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
        [Validation.NotNullOrWhitespace]
        public string ResourceIdentifier { get; set; }

        /// <inheritdoc />
        public override string ToString() => $"{ResourceIdentifier} for workflow form {WorkflowFormId}";
    }
}