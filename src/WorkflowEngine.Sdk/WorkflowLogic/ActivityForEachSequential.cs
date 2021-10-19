using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.AsyncManager.Sdk;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Model;

namespace Nexus.Link.WorkflowEngine.Sdk.WorkflowLogic
{
    public class ActivityForEachSequential<TItemType> : Activity
    {
        public IEnumerable<TItemType> Items { get; }

        public object Result { get; set; }

        public ActivityForEachSequential(ActivityInformation activityInformation,
            IAsyncRequestClient asyncRequestClient, IEnumerable<TItemType> items,
            Activity previousActivity, Activity parentActivity)
            : base(activityInformation, asyncRequestClient, parentActivity)
        {
            Items = items;
            InternalContract.RequireAreEqual(WorkflowActivityTypeEnum.ForEachSequential, ActivityInformation.ActivityType, "Ignore",
                $"The activity {ActivityInformation} was declared as {ActivityInformation.ActivityType}, so you can't use {nameof(ActivityForEachParallel<TItemType>)}.");
        }

        public async Task ExecuteAsync(
            Func<TItemType, ActivityForEachParallel<TItemType>, CancellationToken, Task> method,
            CancellationToken cancellationToken, params object[] arguments)
        {
            foreach (var item in Items)
            {
                await InternalExecuteAsync((instance, ct) => MapMethod(item, method, instance, ct),
                    cancellationToken);
            }
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

    public class ActivityForEachSequential<TActivityReturns, TItemType> : Activity
    {
        private readonly Func<Task<TActivityReturns>> _getDefaultValueMethodAsync;
        public IEnumerable<TItemType> Items { get; }

        public object Result { get; set; }

        public ActivityForEachSequential(ActivityInformation activityInformation,
            IAsyncRequestClient asyncRequestClient, IEnumerable<TItemType> items, Activity parentActivity, Func<Task<TActivityReturns>> getDefaultValueMethodAsync)
            : base(activityInformation, asyncRequestClient, parentActivity)
        {
            _getDefaultValueMethodAsync = getDefaultValueMethodAsync;
            Items = items;
            InternalContract.RequireAreEqual(WorkflowActivityTypeEnum.ForEachSequential, ActivityInformation.ActivityType, "Ignore",
                $"The activity {ActivityInformation} was declared as {ActivityInformation.ActivityType}, so you can't use {nameof(ActivityForEachParallel<TItemType>)}.");
        }

        public async Task<List<TActivityReturns>> ExecuteAsync(
            Func<TItemType, ActivityForEachParallel<TActivityReturns, TItemType>, CancellationToken, Task<TActivityReturns>> method,
            CancellationToken cancellationToken, params object[] arguments)
        {
            var resultList = new List<TActivityReturns>();
            foreach (var item in Items)
            {
                var result = await InternalExecuteAsync((instance, ct) => MapMethod(item, method, instance, ct),
                                    _getDefaultValueMethodAsync, cancellationToken);
                resultList.Add(result);
            }

            return resultList;
        }

        private Task<TActivityReturns> MapMethod(
            TItemType item,
            Func<TItemType, ActivityForEachParallel<TActivityReturns, TItemType>, CancellationToken, Task<TActivityReturns>> method,
            Activity instance, CancellationToken cancellationToken)
        {
            var loop = instance as ActivityForEachParallel<TActivityReturns, TItemType>;
            FulcrumAssert.IsNotNull(loop, CodeLocation.AsString());
            return method(item, loop, cancellationToken);
        }
    }
}