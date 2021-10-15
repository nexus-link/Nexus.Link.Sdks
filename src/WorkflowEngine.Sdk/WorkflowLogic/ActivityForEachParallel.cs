﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.AsyncManager.Sdk;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Model;

namespace Nexus.Link.WorkflowEngine.Sdk.WorkflowLogic
{
    public class ActivityForEachParallel<TItemType> : Activity
    {
        public IEnumerable<TItemType> Items { get; }

        public object Result { get; set; }

        public ActivityForEachParallel(ActivityInformation activityInformation,
            IAsyncRequestClient asyncRequestClient, IEnumerable<TItemType> items,
            Activity previousActivity, Activity parentActivity)
            : base(activityInformation, asyncRequestClient, previousActivity, parentActivity)
        {
            Items = items;
            InternalContract.RequireAreEqual(WorkflowActivityTypeEnum.ForEachParallel, ActivityInformation.ActivityType, "Ignore",
                $"The activity {ActivityInformation} was declared as {ActivityInformation.ActivityType}, so you can't use {nameof(ActivityForEachParallel<TItemType>)}.");
        }

        public List<Task<TMethodReturnType>> ExecuteAsync<TMethodReturnType>(
            Func<TItemType, ActivityForEachParallel<TItemType>, CancellationToken, Task<TMethodReturnType>> method,
            CancellationToken cancellationToken, params object[] arguments)
        {
            InternalContract.Require(
                ActivityInformation.ActivityType == WorkflowActivityTypeEnum.Action ||
                ActivityInformation.ActivityType == WorkflowActivityTypeEnum.ForEachParallel,
                $"The activity {ActivityInformation} was declared as {ActivityInformation.ActivityType}, so you can't call {nameof(ExecuteAsync)}.");

            var taskList = new List<Task<TMethodReturnType>>();
            foreach (var item in Items)
            {
                var task = InternalExecuteAsync((instance, ct) => MapMethod(item, method, instance, ct),
                    cancellationToken);
                taskList.Add(task);
            }

            return taskList;
        }

        public Task ExecuteAsync(
            Func<TItemType, ActivityForEachParallel<TItemType>, CancellationToken, Task> method,
            CancellationToken cancellationToken, params object[] arguments)
        {
            
            var tasks = ExecuteAsync(async (item, a, ct) =>
            {
                await method(item, a, ct);
                return true;
            }, cancellationToken);
            
            return Task.WhenAll(tasks);
        }

        private Task<TMethodReturnType> MapMethod<TMethodReturnType>(
            TItemType item,
            Func<TItemType, ActivityForEachParallel<TItemType>, CancellationToken, Task<TMethodReturnType>> method,
            Activity instance, CancellationToken cancellationToken)
        {
            var loop = instance as ActivityForEachParallel<TItemType>;
            FulcrumAssert.IsNotNull(loop, CodeLocation.AsString());
            return method(item, loop, cancellationToken);
        }
    }
}