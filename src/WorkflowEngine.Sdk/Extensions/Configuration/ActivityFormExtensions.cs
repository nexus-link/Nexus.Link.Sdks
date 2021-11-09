using System;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.Configuration;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Extensions.Configuration
{
    public static class ActivityFormExtensions
    {
        /// <summary>
        /// WorkflowRecord.From(Workflow)
        /// </summary>
        public static ActivityFormRecordCreate From(this ActivityFormRecordCreate target, ActivityFormCreate source)
        {
            InternalContract.RequireNotNull(target, nameof(target));
            InternalContract.RequireNotNull(source, nameof(source));
            InternalContract.RequireValidated(source, nameof(source));
            
            target.WorkflowFormId = source.WorkflowFormId.ToGuid();
            target.Type = source.Type.ToString();
            target.Title = source.Title;
            return target;
        }

        /// <summary>
        /// WorkflowRecord.From(Workflow)
        /// </summary>
        public static ActivityFormRecord From(this ActivityFormRecord target, ActivityForm source)
        {
            InternalContract.RequireNotNull(target, nameof(target));
            InternalContract.RequireNotNull(source, nameof(source));
            InternalContract.RequireValidated(source, nameof(source));

            ((ActivityFormRecordCreate) target).From(source);
            target.Id = source.Id.ToGuid();
            target.Etag = source.Etag;
            return target;
        }

        /// <summary>
        /// Workflow.From(WorkflowRecord)
        /// </summary>
        public static ActivityForm From(this ActivityForm target, ActivityFormRecord source)
        {
            InternalContract.RequireNotNull(target, nameof(target));
            InternalContract.RequireNotNull(source, nameof(source));
            InternalContract.RequireValidated(source, nameof(source));
            
            target.Id = source.Id.ToLowerCaseString();
            target.WorkflowFormId = source.WorkflowFormId.ToLowerCaseString();
            target.Type = source.Type.ToEnum<ActivityTypeEnum>();
            target.Title = source.Title;
            target.Etag = source.Etag;
            return target;
        }
        
    }
}