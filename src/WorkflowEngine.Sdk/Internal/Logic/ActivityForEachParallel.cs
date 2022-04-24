using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Support;
using Nexus.Link.WorkflowEngine.Sdk.Support;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Logic
{
    internal class ActivityForEachParallel<TItemType> : Activity, IActivityForEachParallel<TItemType>
    {
        public IEnumerable<TItemType> Items { get; }

        public ActivityForEachParallel(IActivityInformation activityInformation, IEnumerable<TItemType> items)
            : base(activityInformation)
        {
            InternalContract.RequireNotNull(items, nameof(items));
            Items = items;
            Iteration = 0;
        }

        public async Task ExecuteAsync(
            Func<TItemType, IActivityForEachParallel<TItemType>, CancellationToken, Task> method,
            CancellationToken cancellationToken = default)
        {
            await InternalExecuteAsync(
                (_, ct) => ForEachMethod(method, ct),
                cancellationToken);
        }

        private async Task ForEachMethod(Func<TItemType, IActivityForEachParallel<TItemType>, CancellationToken, Task> method, CancellationToken cancellationToken)
        {
            FulcrumAssert.IsNotNull(Instance.Id, CodeLocation.AsString());
            WorkflowStatic.Context.ParentActivityInstanceId = Instance.Id;
            var taskList = new List<Task>();
            foreach (var item in Items)
            {
                Iteration++;
                ActivityInformation.Workflow.LatestActivity = this;
                var task = MapMethodAsync(item, method, this, cancellationToken);
                taskList.Add(task);
            }
            ActivityInformation.Workflow.LatestActivity = this;

            await WorkflowHelper.WhenAllActivities(taskList);
        }

        private Task MapMethodAsync(
            TItemType item,
            Func<TItemType, IActivityForEachParallel<TItemType>, CancellationToken, Task> method,
            IActivity instance, CancellationToken cancellationToken)
        {
            var loop = instance as IActivityForEachParallel<TItemType>;
            FulcrumAssert.IsNotNull(loop, CodeLocation.AsString());
            return method(item, loop, cancellationToken);
        }
    }

    internal class ActivityForEachParallel<TActivityReturns, TItem> : Activity, IActivityForEachParallel<TActivityReturns, TItem>
    {
        private Func<TItem, string> _getKeyMethod;

        public IEnumerable<TItem> Items { get; }

        public ActivityForEachParallel(
            IActivityInformation activityInformation,
            IEnumerable<TItem> items,
            Func<TItem, string> getKeyMethod)
            : base(activityInformation)
        {
            InternalContract.RequireNotNull(items, nameof(items));
            InternalContract.RequireNotNull(getKeyMethod, nameof(getKeyMethod));
            Items = items;
            Iteration = 0;
            _getKeyMethod = getKeyMethod;
        }

        public Task<IDictionary<string, TActivityReturns>> ExecuteAsync(
            Func<TItem, IActivityForEachParallel<TActivityReturns, TItem>, CancellationToken, Task<TActivityReturns>> method,
            CancellationToken cancellationToken = default)
        {
            return InternalExecuteAsync(
                (a, ct) => ForEachMethod(method, a, ct),
                (_) => null, cancellationToken);
        }

        private Task<IDictionary<string, TActivityReturns>> ForEachMethod(Func<TItem,
                IActivityForEachParallel<TActivityReturns, TItem>, CancellationToken, Task<TActivityReturns>> method,
            IActivity activity, CancellationToken cancellationToken)
        {
            FulcrumAssert.IsNotNull(Instance.Id, CodeLocation.AsString());
            WorkflowStatic.Context.ParentActivityInstanceId = Instance.Id;
            var taskDictionary = new Dictionary<string, Task<TActivityReturns>>();
            foreach (var item in Items)
            {
                Iteration++;
                string key = default;
                try
                {
                    key = _getKeyMethod!(item);
                }
                catch (Exception)
                {
                    InternalContract.Require(false, $"The {nameof(_getKeyMethod)} method failed. You must make it safe, so that it never fails.");
                }
                InternalContract.Require(key != null, $"The {nameof(_getKeyMethod)} method must not return null.");
                ActivityInformation.Workflow.LatestActivity = this;
                var task = MapMethodAsync(item, method, activity, cancellationToken);
                taskDictionary.Add(key!, task);
            }
            ActivityInformation.Workflow.LatestActivity = this;
            return AggregatePostponeExceptions(taskDictionary);
        }

        private Task<TActivityReturns> MapMethodAsync(
            TItem item,
            Func<TItem, IActivityForEachParallel<TActivityReturns, TItem>, CancellationToken, Task<TActivityReturns>> method,
            IActivity instance, CancellationToken cancellationToken)
        {
            var loop = instance as IActivityForEachParallel<TActivityReturns, TItem>;
            FulcrumAssert.IsNotNull(loop, CodeLocation.AsString());
            return method(item, loop, cancellationToken);
        }

        private static async Task<IDictionary<string, TActivityReturns>> AggregatePostponeExceptions(IDictionary<string, Task<TActivityReturns>> taskDictionary)
        {
            await WorkflowHelper.WhenAllActivities(taskDictionary.Values);
            var resultDictionary = new Dictionary<string, TActivityReturns>();
            foreach (var (key, task) in taskDictionary)
            {
                var result = await task;
                resultDictionary.Add(key, result);
            }
            return resultDictionary;
        }
    }
}