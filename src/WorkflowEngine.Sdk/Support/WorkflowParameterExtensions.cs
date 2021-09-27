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
        public static MethodParameterRecordCreate From(this MethodParameterRecordCreate target, WorkflowParameterCreate source)
        {
            InternalContract.RequireNotNull(target, nameof(target));
            InternalContract.RequireNotNull(source, nameof(source));
            InternalContract.RequireValidated(source, nameof(source));
            target.MasterId = MapperHelper.MapToType<Guid, string>(source.WorkflowFormId);
            target.DependentId = source.Name;
            return target;
        }

        /// <summary>
        /// MethodParameterRecord.From(WorkflowParameter)
        /// </summary>
        public static MethodParameterRecord From(this MethodParameterRecord target, WorkflowParameter source)
        {
            InternalContract.RequireNotNull(target, nameof(target));
            InternalContract.RequireNotNull(source, nameof(source));
            InternalContract.RequireValidated(source, nameof(source));

            ((MethodParameterRecordCreate) target).From(source);
            target.Id = MapperHelper.MapToType<Guid, string>(source.Id);
            target.Etag = source.Etag;
            return target;
        }

        /// <summary>
        /// WorkflowParameter.From(MethodParameterRecord)
        /// </summary>
        public static WorkflowParameter From(this WorkflowParameter target, MethodParameterRecord source)
        {
            InternalContract.RequireNotNull(target, nameof(target));
            InternalContract.RequireNotNull(source, nameof(source));
            InternalContract.RequireValidated(source, nameof(source));
            
            target.Id = MapperHelper.MapToType<string, Guid>(source.Id);
            target.WorkflowFormId = MapperHelper.MapToType<string, Guid>(source.MasterId);
            target.Name = source.DependentId;
            target.Etag = source.Etag;
            return target;
        }
        
    }
}