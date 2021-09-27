﻿using System;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Crud.Helpers;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Support
{
    public static class TransitionExtensions
    {
        /// <summary>
        /// WorkflowRecord.From(Workflow)
        /// </summary>
        public static TransitionRecordCreate From(this TransitionRecordCreate target, TransitionCreate source)
        {
            InternalContract.RequireNotNull(target, nameof(target));
            InternalContract.RequireNotNull(source, nameof(source));
            InternalContract.RequireValidated(source, nameof(source));
            
            target.WorkflowVersionId = MapperHelper.MapToType<Guid, string>(source.WorkflowVersionId);
            target.FromActivityVersionId = MapperHelper.MapToType<Guid?, string>(source.FromActivityVersionId);
            target.ToActivityVersionId = MapperHelper.MapToType<Guid?, string>(source.ToActivityVersionId);
            return target;
        }

        /// <summary>
        /// WorkflowRecord.From(Workflow)
        /// </summary>
        public static TransitionRecord From(this TransitionRecord target, Transition source)
        {
            InternalContract.RequireNotNull(target, nameof(target));
            InternalContract.RequireNotNull(source, nameof(source));
            InternalContract.RequireValidated(source, nameof(source));

            ((TransitionRecordCreate) target).From(source);
            target.Id = MapperHelper.MapToType<Guid, string>(source.Id);
            target.Etag = source.Etag;
            return target;
        }

        /// <summary>
        /// Workflow.From(WorkflowRecord)
        /// </summary>
        public static Transition From(this Transition target, TransitionRecord source)
        {
            InternalContract.RequireNotNull(target, nameof(target));
            InternalContract.RequireNotNull(source, nameof(source));
            InternalContract.RequireValidated(source, nameof(source));
            
            target.Id = MapperHelper.MapToType<string, Guid>(source.Id);
            target.Etag = source.Etag;
            target.WorkflowVersionId = MapperHelper.MapToType<string, Guid>(source.WorkflowVersionId);
            target.FromActivityVersionId = MapperHelper.MapToType<string, Guid?>(source.FromActivityVersionId);
            target.ToActivityVersionId = MapperHelper.MapToType<string, Guid?>(source.ToActivityVersionId);
            return target;
        }
        
    }
}