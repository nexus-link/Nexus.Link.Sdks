using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Support;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Extensions.State
{
    internal static class ActivityInstanceExtensions
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
            target.ParentActivityInstanceId = source.ParentActivityInstanceId?.ToGuid();
            target.ResultAsJson = source.ResultAsJson;
            target.ContextAsJson = source.ContextDictionary == null || !source.ContextDictionary.Any()? null : JsonConvert.SerializeObject(source.ContextDictionary);
            target.ExceptionCategory = source.ExceptionCategory?.ToString();
            target.ExceptionFriendlyMessage = source.ExceptionFriendlyMessage;
            target.ExceptionTechnicalMessage = source.ExceptionTechnicalMessage;
            target.FinishedAt = source.FinishedAt;
            target.ExtraAdminCompleted = source.ExtraAdminCompleted;
            target.AbsolutePosition = source.AbsolutePosition;
            target.AsyncRequestId = source.AsyncRequestId;
            target.ExceptionAlertHandled = source.ExceptionAlertHandled;
            target.Iteration = source.Iteration;
            target.IterationTitle = source.IterationTitle;
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

            target.Id = source.Id.ToGuidString();
            target.Etag = source.Etag;
            target.WorkflowInstanceId = source.WorkflowInstanceId.ToGuidString();
            target.ParentIteration = source.ParentIteration;
            target.ActivityVersionId = source.ActivityVersionId.ToGuidString();
            target.ParentActivityInstanceId = source.ParentActivityInstanceId.ToGuidString();
            target.ResultAsJson = source.ResultAsJson;
            target.ContextDictionary = source.ContextAsJson == null 
                ? new Dictionary<string, JToken>() 
                : new JsonSupportNewtonsoft().Deserialize<Dictionary<string, JToken>>(source.ContextAsJson);
            target.State = source.State.ToEnum<ActivityStateEnum>();
            target.ExceptionCategory = source.ExceptionCategory?.ToEnum<ActivityExceptionCategoryEnum>();
            target.ExceptionFriendlyMessage = source.ExceptionFriendlyMessage;
            target.ExceptionTechnicalMessage = source.ExceptionTechnicalMessage;
            target.StartedAt = source.StartedAt;
            target.FinishedAt = source.FinishedAt;
            target.ExtraAdminCompleted = source.ExtraAdminCompleted;
            target.AbsolutePosition = source.AbsolutePosition;
            target.AsyncRequestId = source.AsyncRequestId;
            target.ExceptionAlertHandled = source.ExceptionAlertHandled;
            target.Iteration = source.Iteration;
            target.IterationTitle = source.IterationTitle;
            return target;
        }
        
        /// <summary>
        /// Reset the activity instance as if it has never been executed.
        /// </summary>
        public static void Reset(this ActivityInstance activityInstance)
        {
            activityInstance.State = ActivityStateEnum.Waiting;
            activityInstance.ResultAsJson = null;
            activityInstance.ExceptionCategory = null;
            activityInstance.ExceptionTechnicalMessage = null;
            activityInstance.ExceptionFriendlyMessage = null;
            activityInstance.AsyncRequestId = null;
            activityInstance.ExceptionAlertHandled = false;
            activityInstance.FinishedAt = null;
            activityInstance.ContextDictionary.Clear();
        }

    }
}