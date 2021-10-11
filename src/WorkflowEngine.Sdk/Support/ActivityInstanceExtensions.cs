﻿using System;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Crud.Helpers;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Support
{
    public static class ActivityInstanceExtensions
    {
        /// <summary>
        /// ActivityInstanceRecord.From(ActivityInstance)
        /// </summary>
        public static ActivityInstanceRecordCreate From(this ActivityInstanceRecordCreate target, ActivityInstanceCreate source)
        {
            InternalContract.RequireNotNull(target, nameof(target));
            InternalContract.RequireNotNull(source, nameof(source));
            InternalContract.RequireValidated(source, nameof(source));
            
            target.WorkflowInstanceId = MapperHelper.MapToType<Guid, string>(source.WorkflowInstanceId);
            target.ParentIteration = source.ParentIteration;
            target.ActivityVersionId = MapperHelper.MapToType<Guid, string>(source.ActivityVersionId);
            target.ParentActivityInstanceId = MapperHelper.MapToType<Guid?, string>(source.ParentActivityInstanceId);
            return target;
        }

        /// <summary>
        /// ActivityInstanceRecord.From(ActivityInstance)
        /// </summary>
        public static ActivityInstanceRecord From(this ActivityInstanceRecord target, ActivityInstance source)
        {
            InternalContract.RequireNotNull(target, nameof(target));
            InternalContract.RequireNotNull(source, nameof(source));
            InternalContract.RequireValidated(source, nameof(source));

            ((ActivityInstanceRecordCreate) target).From(source);
            target.Id = MapperHelper.MapToType<Guid, string>(source.Id);
            target.Etag = source.Etag;
            target.HasCompleted = source.HasCompleted;
            target.ResultAsJson = source.ResultAsJson;
            target.ExceptionName = source.ExceptionType;
            target.ExceptionMessage = source.ExceptionMessage;
            target.StartedAt = source.StartedAt;
            target.FinishedAt = source.FinishedAt;
            target.AsyncRequestId = source.AsyncRequestId;
            return target;
        }

        /// <summary>
        /// ActivityInstance.From(ActivityInstanceRecord)
        /// </summary>
        public static ActivityInstance From(this ActivityInstance target, ActivityInstanceRecord source)
        {
            InternalContract.RequireNotNull(target, nameof(target));
            InternalContract.RequireNotNull(source, nameof(source));
            InternalContract.RequireValidated(source, nameof(source));
            
            target.Id = MapperHelper.MapToType<string, Guid>(source.Id);
            target.Etag = source.Etag;
            target.WorkflowInstanceId = MapperHelper.MapToType<string, Guid>(source.WorkflowInstanceId);
            target.ParentIteration = source.ParentIteration;
            target.ActivityVersionId = MapperHelper.MapToType<string, Guid>(source.ActivityVersionId);
            target.ParentActivityInstanceId = MapperHelper.MapToType<string, Guid?>(source.ParentActivityInstanceId);
            target.ResultAsJson = source.ResultAsJson;
            target.ExceptionType = source.ExceptionName;
            target.ExceptionMessage = source.ExceptionMessage;
            target.StartedAt = source.StartedAt;
            target.FinishedAt = source.FinishedAt;
            target.AsyncRequestId = source.AsyncRequestId;
            return target;
        }
        
    }
}