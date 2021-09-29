using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.AsyncManager.Sdk;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Model;

namespace Nexus.Link.WorkflowEngine.Sdk.WorkflowLogic
{
    public class ActivityForEachParallel<TItemType> : Activity
    {
        public IEnumerable<TItemType> Items { get; }
        public readonly Dictionary<int, string> IterationDescriptions = new Dictionary<int, string>();

        public object Result { get; set; }

        public ActivityForEachParallel(IWorkflowCapability workflowCapability,
            IAsyncRequestClient asyncRequestClient,
            ActivityInformation activityInformation, IEnumerable<TItemType> items,
            Activity previousActivity, Activity parentActivity)
            : base(workflowCapability, asyncRequestClient, activityInformation, previousActivity, parentActivity)
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

        private Task<TMethodReturnType> MapMethod<TMethodReturnType>(
            TItemType item,
            Func<TItemType, ActivityForEachParallel<TItemType>, CancellationToken, Task<TMethodReturnType>> method,
            Activity instance, CancellationToken cancellationToken)
        {
            var loop = instance as ActivityForEachParallel<TItemType>;
            FulcrumAssert.IsNotNull(loop, CodeLocation.AsString());
            return method(item, loop, cancellationToken);
        }

        public Task ExecuteAsync(
            Func<TItemType, ActivityForEachParallel<TItemType>, CancellationToken, Task> method,
            CancellationToken cancellationToken, params object[] arguments)
        {
            InternalContract.Require(
                ActivityInformation.ActivityType == WorkflowActivityTypeEnum.Action ||
                ActivityInformation.ActivityType == WorkflowActivityTypeEnum.ForEachParallel,
                $"The activity {ActivityInformation} was declared as {ActivityInformation.ActivityType}, so you can't call {nameof(ExecuteAsync)}.");

            var taskList = new List<Task>();
            foreach (var item in Items)
            {
                Iteration++;
                var task = InternalExecuteAsync((instance, ct) => MapMethod(item, method, instance, ct),
                    cancellationToken);
                taskList.Add(task);
            }

            return Task.WhenAll(taskList);
        }

        private Task MapMethod(
            TItemType item,
            Func<TItemType, ActivityForEachParallel<TItemType>, CancellationToken, Task> method,
            Activity instance, CancellationToken cancellationToken)
        {
            var loop = instance as ActivityForEachParallel<TItemType>;
            FulcrumAssert.IsNotNull(loop, CodeLocation.AsString());
            return method(item, loop, cancellationToken);
        }
    }
}