using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Extensions.State
{
    internal static class WorkflowSemaphoreExtensions
    {
        /// <summary>
        /// WorkflowSemaphoreRecordCreate.From(WorkflowSemaphoreCreate)
        /// </summary>
        public static WorkflowSemaphoreRecordCreate From(this WorkflowSemaphoreRecordCreate target, WorkflowSemaphoreCreate source)
        {
            InternalContract.RequireNotNull(target, nameof(target));
            InternalContract.RequireNotNull(source, nameof(source));
            InternalContract.RequireValidated(source, nameof(source));

            ((WorkflowSemaphoreRecordUnique)target).From(source);
            target.Limit = source.Limit;
            return target;
        }

        /// <summary>
        /// WorkflowSemaphoreRecordCreate.From(WorkflowSemaphoreCreate)
        /// </summary>
        public static WorkflowSemaphoreRecordUnique From(this WorkflowSemaphoreRecordUnique target, WorkflowSemaphoreCreate source)
        {
            InternalContract.RequireNotNull(target, nameof(target));
            InternalContract.RequireNotNull(source, nameof(source));
            InternalContract.RequireValidated(source, nameof(source));
            target.WorkflowFormId = source.WorkflowFormId?.ToGuid();
            target.ResourceIdentifier = source.ResourceIdentifier;
            return target;
        }

        /// <summary>
        /// WorkflowSemaphoreRecordCreate.From(WorkflowSemaphoreCreate)
        /// </summary>
        public static WorkflowSemaphoreQueueRecordCreate From(this WorkflowSemaphoreQueueRecordCreate target, WorkflowSemaphoreRecord semaphore, WorkflowSemaphoreCreate source)
        {
            InternalContract.RequireNotNull(target, nameof(target));
            InternalContract.RequireNotNull(semaphore, nameof(semaphore));
            InternalContract.RequireValidated(semaphore, nameof(semaphore));
            InternalContract.RequireNotNull(source, nameof(source));
            InternalContract.RequireValidated(source, nameof(source));

            ((WorkflowSemaphoreQueueRecordUnique)target).From(semaphore, source);
            target.Raised = false;
            target.ExpirationAfterSeconds = source.ExpirationTime.TotalSeconds;
            target.ExpiresAt = null;

            return target;
        }

        /// <summary>
        /// WorkflowSemaphoreRecordCreate.From(WorkflowSemaphoreCreate)
        /// </summary>
        public static WorkflowSemaphoreQueueRecordUnique From(this WorkflowSemaphoreQueueRecordUnique target, WorkflowSemaphoreRecord semaphore, WorkflowSemaphoreCreate source)
        {
            InternalContract.RequireNotNull(target, nameof(target));
            InternalContract.RequireNotNull(semaphore, nameof(semaphore));
            InternalContract.RequireValidated(semaphore, nameof(semaphore));
            InternalContract.RequireNotNull(source, nameof(source));
            InternalContract.RequireValidated(source, nameof(source));

            target.WorkflowInstanceId = source.WorkflowInstanceId.ToGuid();
            target.WorkflowSemaphoreId = semaphore.Id;
            target.ParentActivityInstanceId = source.ParentActivityInstanceId?.ToGuid();
            target.ParentIteration = source.ParentIteration;
            return target;
        }
    }
}