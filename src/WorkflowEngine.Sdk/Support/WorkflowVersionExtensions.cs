using System;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Crud.Helpers;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Support
{
    public static class WorkflowVersionExtensions
    {
        /// <summary>
        /// WorkflowVersionRecordCreate.From(WorkflowVersionCreate)
        /// </summary>
        public static WorkflowVersionRecordCreate From(this WorkflowVersionRecordCreate target, WorkflowVersionCreate source)
        {
            InternalContract.RequireNotNull(target, nameof(target));
            InternalContract.RequireNotNull(source, nameof(source));
            InternalContract.RequireValidated(source, nameof(source));
            target.WorkflowFormId = MapperHelper.MapToType<Guid, string>(source.WorkflowFormId);
            target.MajorVersion = source.MajorVersion;
            target.MinorVersion = source.MinorVersion;
            target.DynamicCreate = source.DynamicCreate;
            return target;
        }

        /// <summary>
        /// WorkflowVersionRecord.From(WorkflowVersion)
        /// </summary>
        public static WorkflowVersionRecord From(this WorkflowVersionRecord target, WorkflowVersion source)
        {
            InternalContract.RequireNotNull(target, nameof(target));
            InternalContract.RequireNotNull(source, nameof(source));
            InternalContract.RequireValidated(source, nameof(source));

            ((WorkflowVersionRecordCreate) target).From(source);
            target.Id = MapperHelper.MapToType<Guid, string>(source.Id);
            target.Etag = source.Etag;
            return target;
        }

        /// <summary>
        /// WorkflowVersion.From(WorkflowVersionRecord)
        /// </summary>
        public static WorkflowVersion From(this WorkflowVersion target, WorkflowVersionRecord source)
        {
            InternalContract.RequireNotNull(target, nameof(target));
            InternalContract.RequireNotNull(source, nameof(source));
            InternalContract.RequireValidated(source, nameof(source));
            
            target.Id = MapperHelper.MapToType<string, Guid>(source.Id);
            target.WorkflowFormId = MapperHelper.MapToType<string, Guid>(source.WorkflowFormId);
            target.MajorVersion = source.MajorVersion;
            target.MinorVersion = source.MinorVersion;
            target.DynamicCreate = source.DynamicCreate;
            target.Etag = source.Etag;
            return target;
        }
        
    }
}