using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Model;

namespace Nexus.Link.WorkflowEngine.Sdk.WorkflowLogic
{
    public class ActivityForEachSequential<TItemType> : Activity
    {
        public IEnumerable<TItemType> Items { get; }

        public object Result { get; set; }

        public ActivityForEachSequential(ActivityInformation activityInformation,
            IActivityExecutor activityExecutor, IEnumerable<TItemType> items,
            Activity previousActivity, Activity parentActivity)
            : base(activityInformation, activityExecutor, parentActivity)
        {
            InternalContract.RequireAreEqual(WorkflowActivityTypeEnum.ForEachSequential, ActivityInformation.ActivityType, "Ignore",
                $"The activity {ActivityInformation} was declared as {ActivityInformation.ActivityType}, so you can't use {nameof(ActivityForEachParallel<TItemType>)}.");
            Items = items;
            Iteration = 0;
        }

        public async Task ExecuteAsync(
            Func<TItemType, ActivityForEachParallel<TItemType>, CancellationToken, Task> method,
            CancellationToken cancellationToken = default)
        {
            foreach (var item in Items)
            {
                Iteration++;
                await ActivityExecutor.ExecuteAsync((instance, ct) => MapMethodAsync(item, method, instance, ct),
                    cancellationToken);
            }
        }

        private Task MapMethodAsync(
            TItemType item,
            Func<TItemType, ActivityForEachParallel<TItemType>, CancellationToken, Task> method,
            Activity instance, CancellationToken cancellationToken)
        {
            var loop = instance as ActivityForEachParallel<TItemType>;
            FulcrumAssert.IsNotNull(loop, CodeLocation.AsString());
            return method(item, loop, cancellationToken);
        }
    }

    public class ActivityForEachSequential<TActivityReturns, TItemType> : Activity
    {
        private readonly Func<CancellationToken, Task<TActivityReturns>> _getDefaultValueMethodAsync;
        public IEnumerable<TItemType> Items { get; }

        public object Result { get; set; }

        public ActivityForEachSequential(ActivityInformation activityInformation,
            IActivityExecutor activityExecutor, IEnumerable<TItemType> items, Activity parentActivity, Func<CancellationToken, Task<TActivityReturns>> getDefaultValueMethodAsync)
            : base(activityInformation, activityExecutor, parentActivity)
        {
            InternalContract.RequireAreEqual(WorkflowActivityTypeEnum.ForEachSequential, ActivityInformation.ActivityType, "Ignore",
                $"The activity {ActivityInformation} was declared as {ActivityInformation.ActivityType}, so you can't use {nameof(ActivityForEachParallel<TItemType>)}.");
            _getDefaultValueMethodAsync = getDefaultValueMethodAsync;
            Items = items;
            Iteration = 0;
        }

        public async Task<List<TActivityReturns>> ExecuteAsync(
            Func<TItemType, ActivityForEachSequential<TActivityReturns, TItemType>, CancellationToken, Task<TActivityReturns>> method,
            CancellationToken cancellationToken = default)
        {
            var resultList = new List<TActivityReturns>();
            foreach (var item in Items)
            {
                Iteration++;
                var result = await ActivityExecutor.ExecuteAsync((instance, ct) => MapMethodAsync(item, method, instance, ct),
                                    _getDefaultValueMethodAsync, cancellationToken);
                resultList.Add(result);
            }

            return resultList;
        }

        private Task<TActivityReturns> MapMethodAsync(
            TItemType item,
            Func<TItemType, ActivityForEachSequential<TActivityReturns, TItemType>, CancellationToken, Task<TActivityReturns>> method,
            Activity instance, CancellationToken cancellationToken)
        {
            var loop = instance as ActivityForEachSequential<TActivityReturns, TItemType>;
            FulcrumAssert.IsNotNull(loop, CodeLocation.AsString());
            return method(item, loop, cancellationToken);
        }
    }
}