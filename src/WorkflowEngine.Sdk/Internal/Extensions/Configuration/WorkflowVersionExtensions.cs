﻿using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Extensions.Configuration
{
    internal static class WorkflowVersionExtensions
    {
        /// <summary>
        /// WorkflowVersionRecordCreate.From(WorkflowVersionCreate)
        /// </summary>
        public static WorkflowVersionRecordCreate From(this WorkflowVersionRecordCreate target, WorkflowVersionCreate source)
        {
            InternalContract.RequireNotNull(target, nameof(target));
            InternalContract.RequireNotNull(source, nameof(source));
            InternalContract.RequireValidated(source, nameof(source));
            target.WorkflowFormId = source.WorkflowFormId.ToGuid();
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
            target.Id = source.Id.ToGuid();
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
            
            target.Id = source.Id.ToGuidString();
            target.WorkflowFormId = source.WorkflowFormId.ToGuidString();
            target.MajorVersion = source.MajorVersion;
            target.MinorVersion = source.MinorVersion;
            target.DynamicCreate = source.DynamicCreate;
            target.Etag = source.Etag;
            return target;
        }
        
    }
}