using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Support;
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
            IActivityExecutor activityExecutor, IEnumerable<TItemType> items)
            : base(activityInformation, activityExecutor)
        {
            InternalContract.RequireAreEqual(ActivityTypeEnum.ForEachSequential, ActivityInformation.ActivityType, "Ignore",
                $"The activity {ActivityInformation} was declared as {ActivityInformation.ActivityType}, so you can't use {nameof(ActivityForEachParallel<TItemType>)}.");
            Items = items;
            Iteration = 0;
        }

        public Task ExecuteAsync(
            Func<TItemType, ActivityForEachParallel<TItemType>, CancellationToken, Task> method,
            CancellationToken cancellationToken = default)
        {
            return ActivityExecutor.ExecuteAsync(
                (a, ct) => ForEachMethod(method, a, ct),
                cancellationToken);
        }

        private async Task ForEachMethod(Func<TItemType, ActivityForEachParallel<TItemType>, CancellationToken, Task> method, Activity activity, CancellationToken cancellationToken)
        {
            FulcrumAssert.IsNotNull(ActivityInformation.Activity.Instance.Id, CodeLocation.AsString());
            AsyncWorkflowStatic.Context.ParentActivityInstanceId = ActivityInformation.Activity.Instance.Id;
            foreach (var item in Items)
            {
                Iteration++;
                await MapMethodAsync(item, method, activity, cancellationToken);
                FulcrumAssert.IsNotNull(ActivityInformation.Activity.Instance.Id, CodeLocation.AsString());
                ActivityInformation.WorkflowInformation.LatestActivityInstanceId = ActivityInformation.Activity.Instance.Id;
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
            IActivityExecutor activityExecutor, IEnumerable<TItemType> items, Func<CancellationToken, Task<TActivityReturns>> getDefaultValueMethodAsync)
            : base(activityInformation, activityExecutor)
        {
            InternalContract.RequireAreEqual(ActivityTypeEnum.ForEachSequential, ActivityInformation.ActivityType, "Ignore",
                $"The activity {ActivityInformation} was declared as {ActivityInformation.ActivityType}, so you can't use {nameof(ActivityForEachParallel<TItemType>)}.");
            _getDefaultValueMethodAsync = getDefaultValueMethodAsync;
            Items = items;
            Iteration = 0;
        }

        public Task<IList<TActivityReturns>> ExecuteAsync(
            Func<TItemType, ActivityForEachSequential<TActivityReturns, TItemType>, CancellationToken, Task<TActivityReturns>> method,
            CancellationToken cancellationToken = default)
        {
            return ActivityExecutor.ExecuteAsync(
                (a, ct) => ForEachMethod(method, a, ct),
                (ct) =>  null, cancellationToken);
        }

        private async Task<IList<TActivityReturns>> ForEachMethod(Func<TItemType, ActivityForEachSequential<TActivityReturns, TItemType>, CancellationToken, Task<TActivityReturns>> method, Activity activity, CancellationToken cancellationToken)
        {
            FulcrumAssert.IsNotNull(ActivityInformation.Activity.Instance.Id, CodeLocation.AsString());
            AsyncWorkflowStatic.Context.ParentActivityInstanceId = ActivityInformation.Activity.Instance.Id;
            var resultList = new List<TActivityReturns>();
            foreach (var item in Items)
            {
                Iteration++;
                var result = await MapMethodAsync(item, method, activity, cancellationToken);
                resultList.Add(result);
                FulcrumAssert.IsNotNull(ActivityInformation.Activity.Instance.Id, CodeLocation.AsString());
                ActivityInformation.WorkflowInformation.LatestActivityInstanceId = ActivityInformation.Activity.Instance.Id;
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