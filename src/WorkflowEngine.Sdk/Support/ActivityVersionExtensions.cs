using System;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Crud.Helpers;
using WorkflowEngine.Persistence.Abstract.Entities;

namespace WorkflowEngine.Sdk.Support
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
            
            target.WorkflowVersionId = MapperHelper.MapToType<Guid, string>(source.WorkflowVersionId);
            target.Position = source.Position;
            target.ActivityFormId = MapperHelper.MapToType<Guid, string>(source.ActivityFormId);
            target.ParentActivityVersionId = MapperHelper.MapToType<Guid?, string>(source.ParentActivityVersionId);
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
            target.Id = MapperHelper.MapToType<Guid, string>(source.Id);
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
            
            target.Id = MapperHelper.MapToType<string, Guid>(source.Id);
            target.WorkflowVersionId = MapperHelper.MapToType<string, Guid>(source.WorkflowVersionId);
            target.Position = source.Position;
            target.ActivityFormId = MapperHelper.MapToType<string, Guid>(source.ActivityFormId);
            target.ParentActivityVersionId = MapperHelper.MapToType<string, Guid?>(source.ParentActivityVersionId);
            target.Etag = source.Etag;
            return target;
        }
        
    }
}