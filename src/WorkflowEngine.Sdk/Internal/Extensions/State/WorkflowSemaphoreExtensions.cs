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
            target.From((WorkflowSemaphoreUnique) source);
            target.WorkflowInstanceId = source.WorkflowInstanceId.ToGuid();
            target.Raised = source.Raised;
            return target;
        }


        /// <summary>
        /// WorkflowSemaphoreRecordCreate.From(WorkflowSemaphoreCreate)
        /// </summary>
        public static WorkflowSemaphoreRecordUnique From(this WorkflowSemaphoreRecordUnique target, WorkflowSemaphoreUnique source)
        {
            InternalContract.RequireNotNull(target, nameof(target));
            InternalContract.RequireNotNull(source, nameof(source));
            InternalContract.RequireValidated(source, nameof(source));
            target.WorkflowFormId = source.WorkflowFormId.ToGuid();
            target.ResourceIdentifier = source.ResourceIdentifier;
            return target;
        }
    }
}