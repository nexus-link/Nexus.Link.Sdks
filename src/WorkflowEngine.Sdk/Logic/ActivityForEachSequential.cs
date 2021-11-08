using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.Configuration;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Support;

namespace Nexus.Link.WorkflowEngine.Sdk.Logic
{
    public class ActivityForEachSequential<TItem> : Activity, IActivityForEachSequential<TItem>
    {
        public IEnumerable<TItem> Items { get; }

        public ActivityForEachSequential(
            IInternalActivityFlow activityFlow, IEnumerable<TItem> items)
            : base(ActivityTypeEnum.ForEachSequential, activityFlow)
        {
            InternalContract.RequireNotNull(items, nameof(items));
            Items = items;
            Iteration = 0;
        }

        public Task ExecuteAsync(
            Func<TItem, IActivityForEachSequential<TItem>, CancellationToken, Task> method,
            CancellationToken cancellationToken = default)
        {
            return InternalExecuteAsync(
                (a, ct) => ForEachMethod(method, a, ct),
                cancellationToken);
        }

        private async Task ForEachMethod(Func<TItem, IActivityForEachSequential<TItem>, CancellationToken, Task> method, Activity activity, CancellationToken cancellationToken)
        {
            FulcrumAssert.IsNotNull(Instance.Id, CodeLocation.AsString());
            WorkflowStatic.Context.ParentActivityInstanceId = Instance.Id;
            foreach (var item in Items)
            {
                Iteration++;
                await MapMethodAsync(item, method, activity, cancellationToken);
                FulcrumAssert.IsNotNull(Instance.Id, CodeLocation.AsString());
                WorkflowCache.LatestActivity = this;
            }
        }

        private Task MapMethodAsync(
            TItem item,
            Func<TItem, IActivityForEachSequential<TItem>, CancellationToken, Task> method,
            Activity instance, CancellationToken cancellationToken)
        {
            var loop = instance as IActivityForEachSequential<TItem>;
            FulcrumAssert.IsNotNull(loop, CodeLocation.AsString());
            return method(item, loop, cancellationToken);
        }
    }

    public class ActivityForEachSequential<TActivityReturns, TItemType> : Activity, IActivityForEachSequential<TActivityReturns, TItemType>
    {
        private readonly Func<CancellationToken, Task<TActivityReturns>> _getDefaultValueMethodAsync;
        public IEnumerable<TItemType> Items { get; }

        public ActivityForEachSequential(
            IInternalActivityFlow activityFlow, IEnumerable<TItemType> items, Func<CancellationToken, Task<TActivityReturns>> getDefaultValueMethodAsync)
            : base(ActivityTypeEnum.ForEachSequential, activityFlow)
        {
            InternalContract.RequireNotNull(items, nameof(items));
            _getDefaultValueMethodAsync = getDefaultValueMethodAsync;
            Items = items;
            Iteration = 0;
        }

        public Task<IList<TActivityReturns>> ExecuteAsync(
            Func<TItemType, IActivityForEachSequential<TActivityReturns, TItemType>, CancellationToken, Task<TActivityReturns>> method,
            CancellationToken cancellationToken = default)
        {
            return InternalExecuteAsync(
                (a, ct) => ForEachMethod(method, a, ct),
                (ct) => null, cancellationToken);
        }

        private async Task<IList<TActivityReturns>> ForEachMethod(Func<TItemType, IActivityForEachSequential<TActivityReturns, TItemType>, CancellationToken, Task<TActivityReturns>> method, Activity activity, CancellationToken cancellationToken)
        {
            FulcrumAssert.IsNotNull(Instance.Id, CodeLocation.AsString());
            WorkflowStatic.Context.ParentActivityInstanceId = Instance.Id;
            var resultList = new List<TActivityReturns>();
            foreach (var item in Items)
            {
                Iteration++;
                var result = await MapMethodAsync(item, method, activity, cancellationToken);
                resultList.Add(result);
                FulcrumAssert.IsNotNull(Instance.Id, CodeLocation.AsString());
                WorkflowCache.LatestActivity = this;
            }

            return resultList;
        }

        private Task<TActivityReturns> MapMethodAsync(
            TItemType item,
            Func<TItemType, IActivityForEachSequential<TActivityReturns, TItemType>, CancellationToken, Task<TActivityReturns>> method,
            Activity instance, CancellationToken cancellationToken)
        {
            var loop = instance as IActivityForEachSequential<TActivityReturns, TItemType>;
            FulcrumAssert.IsNotNull(loop, CodeLocation.AsString());
            return method(item, loop, cancellationToken);
        }
    }
}