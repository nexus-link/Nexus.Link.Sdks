using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Extensions.Configuration
{
    public static class WorkflowParameterExtensions
    {
        /// <summary>
        /// MethodParameterRecordCreate.From(WorkflowParameterCreate)
        /// </summary>
        public static WorkflowVersionParameterRecordCreate From(this WorkflowVersionParameterRecordCreate target, WorkflowParameterCreate source)
        {
            InternalContract.RequireNotNull(target, nameof(target));
            InternalContract.RequireNotNull(source, nameof(source));
            InternalContract.RequireValidated(source, nameof(source));
            target.WorkflowVersionId = source.WorkflowVersionId.ToGuid();
            target.Name = source.Name;
            return target;
        }

        /// <summary>
        /// MethodParameterRecord.From(WorkflowParameter)
        /// </summary>
        public static WorkflowVersionParameterRecord From(this WorkflowVersionParameterRecord target, WorkflowParameter source)
        {
            InternalContract.RequireNotNull(target, nameof(target));
            InternalContract.RequireNotNull(source, nameof(source));
            InternalContract.RequireValidated(source, nameof(source));

            ((WorkflowVersionParameterRecordCreate) target).From(source);
            target.Id = source.Id.ToGuid();
            target.Etag = source.Etag;
            return target;
        }

        /// <summary>
        /// WorkflowParameter.From(MethodParameterRecord)
        /// </summary>
        public static WorkflowParameter From(this WorkflowParameter target, WorkflowVersionParameterRecord source)
        {
            InternalContract.RequireNotNull(target, nameof(target));
            InternalContract.RequireNotNull(source, nameof(source));
            InternalContract.RequireValidated(source, nameof(source));
            
            target.Id = source.Id.ToLowerCaseString();
            target.WorkflowVersionId = source.WorkflowVersionId.ToLowerCaseString();
            target.Name = source.Name;
            target.Etag = source.Etag;
            return target;
        }
        
    }
}