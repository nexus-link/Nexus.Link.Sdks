using System;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Crud.Helpers;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Extensions
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
            target.WorkflowVersionId = MapperHelper.MapToType<Guid, string>(source.WorkflowVersionId);
            target.Title = source.Title;
            target.InitialVersion = source.InitialVersion;
            target.StartedAt = source.StartedAt;
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
            target.Id = MapperHelper.MapToType<Guid, string>(source.Id);
            target.Etag = source.Etag;
            target.FinishedAt = source.FinishedAt;
            target.CancelledAt = source.CancelledAt;
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
            
            target.Id = MapperHelper.MapToType<string, Guid>(source.Id);
            target.Etag = source.Etag;
            target.WorkflowVersionId = MapperHelper.MapToType<string, Guid>(source.WorkflowVersionId);
            target.Title = source.Title;
            target.InitialVersion = source.InitialVersion;
            target.StartedAt = source.StartedAt;
            target.FinishedAt = source.FinishedAt;
            target.CancelledAt = source.CancelledAt;
            return target;
        }
        
    }
}