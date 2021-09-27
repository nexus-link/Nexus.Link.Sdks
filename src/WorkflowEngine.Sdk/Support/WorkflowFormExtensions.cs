using System;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Crud.Helpers;
using WorkflowEngine.Persistence.Abstract.Entities;

namespace WorkflowEngine.Sdk.Support
{
    public static class WorkflowFormExtensions
    {
        /// <summary>
        /// WorkflowRecord.From(Workflow)
        /// </summary>
        public static WorkflowFormRecordCreate From(this WorkflowFormRecordCreate target, WorkflowFormCreate source)
        {
            InternalContract.RequireNotNull(target, nameof(target));
            InternalContract.RequireNotNull(source, nameof(source));
            InternalContract.RequireValidated(source, nameof(source));
            
            target.CapabilityName = source.CapabilityName;
            target.Title = source.Title;
            return target;
        }

        /// <summary>
        /// WorkflowRecord.From(Workflow)
        /// </summary>
        public static WorkflowFormRecord From(this WorkflowFormRecord target, WorkflowForm source)
        {
            InternalContract.RequireNotNull(target, nameof(target));
            InternalContract.RequireNotNull(source, nameof(source));
            InternalContract.RequireValidated(source, nameof(source));

            ((WorkflowFormRecordCreate) target).From(source);
            target.Id = MapperHelper.MapToType<Guid, string>(source.Id);
            target.Etag = source.Etag;
            return target;
        }

        /// <summary>
        /// Workflow.From(WorkflowRecord)
        /// </summary>
        public static WorkflowForm From(this WorkflowForm target, WorkflowFormRecord source)
        {
            InternalContract.RequireNotNull(target, nameof(target));
            InternalContract.RequireNotNull(source, nameof(source));
            InternalContract.RequireValidated(source, nameof(source));
            
            target.Id = MapperHelper.MapToType<string, Guid>(source.Id);
            target.Title = source.Title;
            target.CapabilityName = source.CapabilityName;
            target.Etag = source.Etag;
            return target;
        }
        
    }
}