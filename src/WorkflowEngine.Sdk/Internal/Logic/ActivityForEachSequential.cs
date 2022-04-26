using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Support;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Logic
{
    internal class ActivityForEachSequential<TItem> : Activity, IActivityForEachSequential<TItem>
    {
        public IEnumerable<TItem> Items { get; }

        public ActivityForEachSequential(
            IActivityInformation activityInformation, IEnumerable<TItem> items)
            : base(activityInformation)
        {
            InternalContract.RequireNotNull(items, nameof(items));
            Items = items;
            Iteration = 0;
        }

        public Task ExecuteAsync(
            Func<TItem, IActivityForEachSequential<TItem>, CancellationToken, Task> method,
            CancellationToken cancellationToken = default)
        {
            return ActivityExecutor.ExecuteWithoutReturnValueAsync(ct => ForEachMethod(method, ct),
                cancellationToken);
        }

        private async Task ForEachMethod(Func<TItem, IActivityForEachSequential<TItem>, CancellationToken, Task> method, CancellationToken cancellationToken)
        {
            FulcrumAssert.IsNotNull(Instance.Id, CodeLocation.AsString());
            WorkflowStatic.Context.ParentActivityInstanceId = Instance.Id;
            foreach (var item in Items)
            {
                Iteration++;
                await method(item, this, cancellationToken);
                FulcrumAssert.IsNotNull(Instance.Id, CodeLocation.AsString());
                ActivityInformation.Workflow.LatestActivity = this;
            }
        }
    }

    internal class ActivityForEachSequential<TActivityReturns, TItemType> : Activity, IActivityForEachSequential<TActivityReturns, TItemType>
    {
        private readonly Func<CancellationToken, Task<TActivityReturns>> _getDefaultValueMethodAsync;
        public IEnumerable<TItemType> Items { get; }

        public ActivityForEachSequential(
            IActivityInformation activityInformation, IEnumerable<TItemType> items, Func<CancellationToken, Task<TActivityReturns>> getDefaultValueMethodAsync)
            : base(activityInformation)
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
            return ActivityExecutor.ExecuteWithReturnValueAsync(ct => ForEachMethod(method, ct),
                (ct) => null, cancellationToken);
        }

        private async Task<IList<TActivityReturns>> ForEachMethod(
            Func<TItemType, IActivityForEachSequential<TActivityReturns, TItemType>, CancellationToken, Task<TActivityReturns>> method,
            CancellationToken cancellationToken)
        {
            FulcrumAssert.IsNotNull(Instance.Id, CodeLocation.AsString());
            WorkflowStatic.Context.ParentActivityInstanceId = Instance.Id;
            var resultList = new List<TActivityReturns>();
            foreach (var item in Items)
            {
                Iteration++;
                var result = await method(item, this, cancellationToken);
                resultList.Add(result);
                FulcrumAssert.IsNotNull(Instance.Id, CodeLocation.AsString());
                ActivityInformation.Workflow.LatestActivity = this;
            }

            return resultList;
        }
    }
}