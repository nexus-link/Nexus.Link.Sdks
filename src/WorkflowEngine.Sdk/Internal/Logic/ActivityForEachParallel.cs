using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Support;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Logic
{
    internal class ActivityForEachParallel<TItemType> : Activity, IActivityForEachParallel<TItemType>
    {
        public IEnumerable<TItemType> Items { get; }

        public ActivityForEachParallel(IInternalActivityFlow activityFlow, IEnumerable<TItemType> items)
            : base(ActivityTypeEnum.ForEachParallel, activityFlow)
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
                (a, ct) => ForEachMethod(method, a, ct),
                cancellationToken);
        }

        private async Task ForEachMethod(Func<TItemType, IActivityForEachParallel<TItemType>, CancellationToken, Task> method, IActivity activity, CancellationToken cancellationToken)
        {
            FulcrumAssert.IsNotNull(Instance.Id, CodeLocation.AsString());
            WorkflowStatic.Context.ParentActivityInstanceId = Instance.Id;
            var taskList = new List<Task>();
            foreach (var item in Items)
            {
                Iteration++;
                FulcrumAssert.IsNotNull(Instance.Id, CodeLocation.AsString());
                WorkflowCache.LatestActivity = this;
                var task = MapMethodAsync(item, method, activity, cancellationToken);
                taskList.Add(task);
            }
            FulcrumAssert.IsNotNull(Instance.Id, CodeLocation.AsString());
            WorkflowCache.LatestActivity = this;

            await AggregatePostponeExceptions(taskList);
        }

        private static async Task AggregatePostponeExceptions(IList<Task> taskList)
        {
            RequestPostponedException outException = null;
            var current = 0;
            while (taskList.Count > current)
            {
                try
                {
                    await taskList[current];
                    current++;
                }
                catch (ExceptionTransporter et)
                {
                    if (et.InnerException is RequestPostponedException rpe)
                    {
                        outException ??= new RequestPostponedException();
                        outException.AddWaitingForIds(rpe.WaitingForRequestIds);
                        if (!outException.TryAgain) outException.TryAgain = rpe.TryAgain;
                        taskList.RemoveAt(current);
                    }
                    else current++;
                }
            }

            if (outException != null) throw outException;
            await Task.WhenAll(taskList);
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

    internal class ActivityForEachParallel<TActivityReturns, TItem, TKey> : Activity, IActivityForEachParallel<TActivityReturns, TItem, TKey>
    {
        private readonly Func<CancellationToken, Task<TActivityReturns>> _getDefaultValueMethodAsync;

        private Func<TItem, TKey> _getKeyMethod;

        public IEnumerable<TItem> Items { get; }

        public ActivityForEachParallel(
            IInternalActivityFlow activityFlow, IEnumerable<TItem> items, Func<CancellationToken, Task<TActivityReturns>> getDefaultValueMethodAsync)
            : base(ActivityTypeEnum.ForEachParallel, activityFlow)
        {
            InternalContract.RequireNotNull(items, nameof(items));
            _getDefaultValueMethodAsync = getDefaultValueMethodAsync;
            Items = items;
            Iteration = 0;
            if (typeof(TKey).IsAssignableFrom(typeof(TItem)))
            {
                _getKeyMethod = item => (TKey)(object)item;
            }
        }

        public IActivityForEachParallel<TActivityReturns, TItem, TKey> SetGetKeyMethod(Func<TItem, TKey> method)
        {
            _getKeyMethod = method;
            return this;
        }

        public Task<IDictionary<TKey, TActivityReturns>> ExecuteAsync(
            Func<TItem, IActivityForEachParallel<TActivityReturns, TItem, TKey>, CancellationToken, Task<TActivityReturns>> method,
            CancellationToken cancellationToken = default)
        {
            return InternalExecuteAsync(
                (a, ct) => ForEachMethod(method, a, ct),
                (ct) => null, cancellationToken);
        }

        private Task<IDictionary<TKey, TActivityReturns>> ForEachMethod(Func<TItem,
                IActivityForEachParallel<TActivityReturns, TItem, TKey>, CancellationToken, Task<TActivityReturns>> method,
            IActivity activity, CancellationToken cancellationToken)
        {
            InternalContract.Require(_getKeyMethod != null, $"You must call {nameof(SetGetKeyMethod)} before you call the {nameof(ExecuteAsync)} method.");
            FulcrumAssert.IsNotNull(Instance.Id, CodeLocation.AsString());
            WorkflowStatic.Context.ParentActivityInstanceId = Instance.Id;
            var taskDictionary = new Dictionary<TKey, Task<TActivityReturns>>();
            foreach (var item in Items)
            {
                Iteration++;
                TKey key = default;
                try
                {
                    key = _getKeyMethod!(item);
                }
                catch (Exception)
                {
                    InternalContract.Require(false, $"The {nameof(_getKeyMethod)} method failed. You must make it safe, so that it never fails.");
                }
                InternalContract.Require(key != null, $"The {nameof(_getKeyMethod)} method must not return null.");
                FulcrumAssert.IsNotNull(Instance.Id, CodeLocation.AsString());
                WorkflowCache.LatestActivity = this;
                var task = MapMethodAsync(item, method, activity, cancellationToken);
                taskDictionary.Add(key!, task);
            }
            FulcrumAssert.IsNotNull(Instance.Id, CodeLocation.AsString());
            WorkflowCache.LatestActivity = this;
            return AggregatePostponeExceptions(taskDictionary);
        }

        private Task<TActivityReturns> MapMethodAsync(
            TItem item,
            Func<TItem, IActivityForEachParallel<TActivityReturns, TItem, TKey>, CancellationToken, Task<TActivityReturns>> method,
            IActivity instance, CancellationToken cancellationToken)
        {
            var loop = instance as IActivityForEachParallel<TActivityReturns, TItem, TKey>;
            FulcrumAssert.IsNotNull(loop, CodeLocation.AsString());
            return method(item, loop, cancellationToken);
        }

        private static async Task<IDictionary<TKey, TActivityReturns>> AggregatePostponeExceptions(IDictionary<TKey, Task<TActivityReturns>> taskDictionary)
        {
            RequestPostponedException outException = null;
            var resultDictionary = new Dictionary<TKey, TActivityReturns>();
            foreach (var (key, task) in taskDictionary)
            {
                try
                {
                    var result = await task;
                    resultDictionary.Add(key, result);
                }
                catch (ExceptionTransporter et)
                {
                    if (et.InnerException is RequestPostponedException rpe)
                    {
                        outException ??= new RequestPostponedException();
                        outException.AddWaitingForIds(rpe.WaitingForRequestIds);
                        if (!outException.TryAgain) outException.TryAgain = rpe.TryAgain;
                    }
                }
            }

            if (outException != null) throw outException;
            return resultDictionary;
        }
    }
}