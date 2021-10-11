﻿using System;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Crud.Helpers;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Support
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
            
            target.WorkflowFormId = MapperHelper.MapToType<Guid, string>(source.WorkflowFormId);
            target.Type = source.Type;
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
            target.Id = MapperHelper.MapToType<Guid, string>(source.Id);
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
            
            target.Id = MapperHelper.MapToType<string, Guid>(source.Id);
            target.WorkflowFormId = MapperHelper.MapToType<string, Guid>(source.WorkflowFormId);
            target.Type = source.Type;
            target.Title = source.Title;
            target.Etag = source.Etag;
            return target;
        }
        
    }
}