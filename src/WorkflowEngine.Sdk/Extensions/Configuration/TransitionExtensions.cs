using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Extensions.Configuration
{
    public static class TransitionExtensions
    {
        /// <summary>
        /// WorkflowRecord.From(Workflow)
        /// </summary>
        public static TransitionRecordUnique From(this TransitionRecordUnique target, TransitionUnique source)
        {
            InternalContract.RequireNotNull(target, nameof(target));
            InternalContract.RequireNotNull(source, nameof(source));
            InternalContract.RequireValidated(source, nameof(source));
            
            target.WorkflowVersionId = source.WorkflowVersionId.ToGuid();
            target.FromActivityVersionId = source.FromActivityVersionId?.ToGuid();
            target.ToActivityVersionId = source.ToActivityVersionId?.ToGuid();
            return target;
        }

        /// <summary>
        /// WorkflowRecord.From(Workflow)
        /// </summary>
        public static TransitionRecordCreate From(this TransitionRecordCreate target, TransitionCreate source)
        {
            InternalContract.RequireNotNull(target, nameof(target));
            InternalContract.RequireNotNull(source, nameof(source));
            InternalContract.RequireValidated(source, nameof(source));

            ((TransitionRecordUnique) target).From(source);
            return target;
        }

        /// <summary>
        /// WorkflowRecord.From(Workflow)
        /// </summary>
        public static TransitionRecord From(this TransitionRecord target, Transition source)
        {
            InternalContract.RequireNotNull(target, nameof(target));
            InternalContract.RequireNotNull(source, nameof(source));
            InternalContract.RequireValidated(source, nameof(source));

            ((TransitionRecordCreate) target).From(source);
            target.Id = source.Id.ToGuid();
            target.Etag = source.Etag;
            return target;
        }

        /// <summary>
        /// Workflow.From(WorkflowRecord)
        /// </summary>
        public static Transition From(this Transition target, TransitionRecord source)
        {
            InternalContract.RequireNotNull(target, nameof(target));
            InternalContract.RequireNotNull(source, nameof(source));
            InternalContract.RequireValidated(source, nameof(source));
            
            target.Id = source.Id.ToLowerCaseString();
            target.Etag = source.Etag;
            target.WorkflowVersionId = source.WorkflowVersionId.ToLowerCaseString();
            target.FromActivityVersionId = source.FromActivityVersionId.ToLowerCaseString();
            target.ToActivityVersionId = source.ToActivityVersionId.ToLowerCaseString();
            return target;
        }
        
    }
}