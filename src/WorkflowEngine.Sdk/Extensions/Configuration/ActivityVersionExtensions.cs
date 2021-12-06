using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Extensions.Configuration
{
    public static class ActivityVersionExtensions
    {
        /// <summary>
        /// WorkflowRecord.From(Workflow)
        /// </summary>
        public static ActivityVersionRecordCreate From(this ActivityVersionRecordCreate target, ActivityVersionCreate source)
        {
            InternalContract.RequireNotNull(target, nameof(target));
            InternalContract.RequireNotNull(source, nameof(source));
            InternalContract.RequireValidated(source, nameof(source));
            
            target.WorkflowVersionId = source.WorkflowVersionId.ToGuid();
            target.Position = source.Position;
            target.ActivityFormId = source.ActivityFormId.ToGuid();
            target.ParentActivityVersionId = source.ParentActivityVersionId?.ToGuid();
            target.FailUrgency = source.FailUrgency.ToString();
            return target;
        }

        /// <summary>
        /// WorkflowRecord.From(Workflow)
        /// </summary>
        public static ActivityVersionRecord From(this ActivityVersionRecord target, ActivityVersion source)
        {
            InternalContract.RequireNotNull(target, nameof(target));
            InternalContract.RequireNotNull(source, nameof(source));
            InternalContract.RequireValidated(source, nameof(source));

            ((ActivityVersionRecordCreate) target).From(source);
            target.Id = source.Id.ToGuid();
            target.Etag = source.Etag;
            return target;
        }

        /// <summary>
        /// Workflow.From(WorkflowRecord)
        /// </summary>
        public static ActivityVersion From(this ActivityVersion target, ActivityVersionRecord source)
        {
            InternalContract.RequireNotNull(target, nameof(target));
            InternalContract.RequireNotNull(source, nameof(source));
            InternalContract.RequireValidated(source, nameof(source));
            
            target.Id = source.Id.ToLowerCaseString();
            target.WorkflowVersionId = source.WorkflowVersionId.ToLowerCaseString();
            target.Position = source.Position;
            target.ActivityFormId = source.ActivityFormId.ToLowerCaseString();
            target.ParentActivityVersionId = source.ParentActivityVersionId.ToLowerCaseString();
            target.FailUrgency = source.FailUrgency.ToEnum<ActivityFailUrgencyEnum>();
            target.Etag = source.Etag;
            return target;
        }
        
    }
}