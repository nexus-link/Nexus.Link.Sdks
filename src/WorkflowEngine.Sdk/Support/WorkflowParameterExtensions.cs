using System;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Crud.Helpers;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Support
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
            target.WorkflowVersionId = MapperHelper.MapToType<Guid, string>(source.WorkflowVersionId);
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
            target.Id = MapperHelper.MapToType<Guid, string>(source.Id);
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
            
            target.Id = MapperHelper.MapToType<string, Guid>(source.Id);
            target.WorkflowVersionId = MapperHelper.MapToType<string, Guid>(source.WorkflowVersionId);
            target.Name = source.Name;
            target.Etag = source.Etag;
            return target;
        }
        
    }
}