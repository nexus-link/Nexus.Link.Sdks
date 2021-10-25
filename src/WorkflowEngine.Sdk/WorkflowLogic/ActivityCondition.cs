﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Persistence;

namespace Nexus.Link.WorkflowEngine.Sdk.WorkflowLogic
{
    public class ActivityCondition<TActivityReturns> : Activity
    {
        private readonly Func<CancellationToken, Task<TActivityReturns>> _getDefaultValueMethodAsync;

        public ActivityCondition(ActivityPersistence activityPersistence, IActivityExecutor activityExecutor, Func<CancellationToken, Task<TActivityReturns>> getDefaultValueMethodAsync)
            : base(activityPersistence, activityExecutor)
        {
            _getDefaultValueMethodAsync = getDefaultValueMethodAsync;
            InternalContract.RequireAreEqual(ActivityTypeEnum.Condition, ActivityPersistence.ActivityType, "Ignore",
                $"The activity {ActivityPersistence} was declared as {ActivityPersistence.ActivityType}, so you can't use {nameof(ActivityCondition<TActivityReturns>)}.");
        }

        public Task<TActivityReturns> ExecuteAsync(
            Func<Activity, CancellationToken, Task<TActivityReturns>> conditionMethodAsync,
            CancellationToken cancellationToken = default)
        {
            return ActivityExecutor.ExecuteAsync((instance, ct) => MapMethodAsync(conditionMethodAsync, instance, ct), _getDefaultValueMethodAsync, cancellationToken);
        }

        private static Task<TActivityReturns> MapMethodAsync(
            Func<ActivityCondition<TActivityReturns>, CancellationToken, Task<TActivityReturns>> method, 
            Activity instance, CancellationToken cancellationToken)
        {
            var condition = instance as ActivityCondition<TActivityReturns>;
            FulcrumAssert.IsNotNull(condition, CodeLocation.AsString());
            return method(condition, cancellationToken);
        }
    }
}