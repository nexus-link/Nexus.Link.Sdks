using System;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.State;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Extensions.State
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

            target.WorkflowInstanceId = source.WorkflowInstanceId.ToGuid();
            target.ParentIteration = source.ParentIteration;
            target.State = source.State.ToString();
            target.StartedAt = source.StartedAt;
            target.ActivityVersionId = source.ActivityVersionId.ToGuid();
            target.ParentActivityInstanceId = source.ParentActivityInstanceId.ToNullableGuid();
            target.ResultAsJson = source.ResultAsJson;
            target.ExceptionCategory = source.ExceptionCategory?.ToString();
            target.ExceptionFriendlyMessage = source.ExceptionFriendlyMessage;
            target.ExceptionTechnicalMessage = source.ExceptionTechnicalMessage;
            target.FinishedAt = source.FinishedAt;
            target.AsyncRequestId = source.AsyncRequestId;
            target.ExceptionAlertHandled = source.ExceptionAlertHandled;
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

            ((ActivityInstanceRecordCreate)target).From(source);
            target.Id = source.Id.ToGuid();
            target.Etag = source.Etag;
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

            target.Id = source.Id.ToLowerCaseString();
            target.Etag = source.Etag;
            target.WorkflowInstanceId = source.WorkflowInstanceId.ToLowerCaseString();
            target.ParentIteration = source.ParentIteration;
            target.ActivityVersionId = source.ActivityVersionId.ToLowerCaseString();
            target.ParentActivityInstanceId = source.ParentActivityInstanceId.ToLowerCaseString();
            target.ResultAsJson = source.ResultAsJson;
            target.State = source.State.ToEnum<ActivityStateEnum>();
            target.ExceptionCategory = source.ExceptionCategory?.ToEnum<ActivityExceptionCategoryEnum>();
            target.ExceptionFriendlyMessage = source.ExceptionFriendlyMessage;
            target.ExceptionTechnicalMessage = source.ExceptionTechnicalMessage;
            target.StartedAt = source.StartedAt;
            target.FinishedAt = source.FinishedAt;
            target.AsyncRequestId = source.AsyncRequestId;
            target.ExceptionAlertHandled = source.ExceptionAlertHandled;
            return target;
        }

    }
}