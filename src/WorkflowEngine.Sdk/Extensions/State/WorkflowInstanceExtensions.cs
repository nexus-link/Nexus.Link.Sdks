using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Extensions.State
{
    public static class WorkflowInstanceExtensions
    {
        /// <summary>
        /// WorkflowInstanceRecordCreate.From(WorkflowInstanceCreate)
        /// </summary>
        public static WorkflowInstanceRecordCreate From(this WorkflowInstanceRecordCreate target, WorkflowInstanceCreate source)
        {
            InternalContract.RequireNotNull(target, nameof(target));
            InternalContract.RequireNotNull(source, nameof(source));
            InternalContract.RequireValidated(source, nameof(source));
            target.WorkflowVersionId = source.WorkflowVersionId.ToGuid();
            target.Title = source.Title;
            target.InitialVersion = source.InitialVersion;
            target.StartedAt = source.StartedAt;
            target.State = source.State.ToString();
            target.FinishedAt = source.FinishedAt;
            target.CancelledAt = source.CancelledAt;
            target.ResultAsJson = source.ResultAsJson;
            target.ExceptionFriendlyMessage = source.ExceptionFriendlyMessage;
            target.ExceptionTechnicalMessage = source.ExceptionTechnicalMessage;
            target.IsComplete = source.IsComplete;
            return target;
        }

        /// <summary>
        /// WorkflowInstanceRecord.From(WorkflowInstance)
        /// </summary>
        public static WorkflowInstanceRecord From(this WorkflowInstanceRecord target, WorkflowInstance source)
        {
            InternalContract.RequireNotNull(target, nameof(target));
            InternalContract.RequireNotNull(source, nameof(source));
            InternalContract.RequireValidated(source, nameof(source));

            ((WorkflowInstanceRecordCreate) target).From(source);
            target.Id = source.Id.ToGuid();
            target.Etag = source.Etag;
            return target;
        }

        /// <summary>
        /// WorkflowInstance.From(WorkflowInstanceRecord)
        /// </summary>
        public static WorkflowInstance From(this WorkflowInstance target, WorkflowInstanceRecord source)
        {
            InternalContract.RequireNotNull(target, nameof(target));
            InternalContract.RequireNotNull(source, nameof(source));
            InternalContract.RequireValidated(source, nameof(source));
            
            target.Id = source.Id.ToGuidString();
            target.Etag = source.Etag;
            target.WorkflowVersionId = source.WorkflowVersionId.ToGuidString();
            target.Title = source.Title;
            target.InitialVersion = source.InitialVersion;
            target.StartedAt = source.StartedAt;
            target.FinishedAt = source.FinishedAt;
            target.State = source.State.ToEnum<WorkflowStateEnum>();
            target.CancelledAt = source.CancelledAt;
            target.ResultAsJson = source.ResultAsJson;
            target.ExceptionFriendlyMessage = source.ExceptionFriendlyMessage;
            target.ExceptionTechnicalMessage = source.ExceptionTechnicalMessage;
            target.IsComplete = source.IsComplete;
            return target;
        }
        
    }
}