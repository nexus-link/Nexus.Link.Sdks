﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.AsyncManager.Sdk;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Model;

namespace Nexus.Link.WorkflowEngine.Sdk.WorkflowLogic
{
    public class ActivityIf : Activity
    {
        public ActivityIf(IWorkflowCapability workflowCapability,
            IAsyncRequestClient asyncRequestClient, ActivityInformation activityInformation, 
            Activity previousActivity, Activity parentActivity)
            :base(workflowCapability, asyncRequestClient, activityInformation, previousActivity, parentActivity)
        {
            InternalContract.RequireAreEqual(WorkflowActivityTypeEnum.IfThenElse, ActivityInformation.ActivityType, "Ignore",
                $"The activity {ActivityInformation} was declared as {ActivityInformation.ActivityType}, so you can't use {nameof(ActivityIf)}.");
        }

        private static Task<TMethodReturnType> MapMethod<TMethodReturnType>(
            Func<ActivityIf, CancellationToken, Task<TMethodReturnType>> method, 
            Activity instance, CancellationToken cancellationToken)
        {
            var condition = instance as ActivityIf;
            FulcrumAssert.IsNotNull(condition, CodeLocation.AsString());
            return method(condition, cancellationToken);
        }

        public Task<bool> IfAsync(
            Func<Activity, CancellationToken, Task<bool>> ifMethodAsync,
            CancellationToken cancellationToken)
        {
            return ExecuteMethod(ifMethodAsync, cancellationToken);
        }

        private Task<TMethodReturnType> ExecuteMethod<TMethodReturnType>(Func<Activity, CancellationToken, Task<TMethodReturnType>> methodAsync, CancellationToken cancellationToken)
        {
            return InternalExecuteAsync((instance, ct) => MapMethod(methodAsync, instance, ct), cancellationToken);
        }
    }
}