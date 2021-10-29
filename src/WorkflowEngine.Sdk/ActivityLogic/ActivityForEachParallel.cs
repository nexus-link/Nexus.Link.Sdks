﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.Configuration;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Support;

namespace Nexus.Link.WorkflowEngine.Sdk.ActivityLogic
{
    public class ActivityForEachParallel<TItemType> : Activity
    {
        public IEnumerable<TItemType> Items { get; }

        public object Result { get; set; }

        public ActivityForEachParallel(IInternalActivityFlow activityFlow, IEnumerable<TItemType> items)
            : base(ActivityTypeEnum.ForEachParallel, activityFlow)
        {
            InternalContract.RequireNotNull(items, nameof(items));
            Items = items;
            Iteration = 0;
        }

        public async Task ExecuteAsync(
            Func<TItemType, ActivityForEachParallel<TItemType>, CancellationToken, Task> method,
            CancellationToken cancellationToken = default)
        {
            await InternalExecuteAsync(
                (a, ct) => ForEachMethod(method, a, ct),
                cancellationToken);
        }

        private async Task ForEachMethod(Func<TItemType, ActivityForEachParallel<TItemType>, CancellationToken, Task> method, Activity activity, CancellationToken cancellationToken)
        {
            FulcrumAssert.IsNotNull(ActivityPersistence.ActivitySummary.Instance.Id, CodeLocation.AsString());
            AsyncWorkflowStatic.Context.ParentActivityInstanceId = ActivityPersistence.ActivitySummary.Instance.Id;
            var taskList = new List<Task>();
            foreach (var item in Items)
            {
                Iteration++;
                FulcrumAssert.IsNotNull(ActivityPersistence.ActivitySummary.Instance.Id, CodeLocation.AsString());
                ActivityPersistence.WorkflowPersistence.LatestActivityInstanceId = ActivityPersistence.ActivitySummary.Instance.Id;
                var task = MapMethodAsync(item, method, activity, cancellationToken);
                taskList.Add(task);
            }
            FulcrumAssert.IsNotNull(ActivityPersistence.ActivitySummary.Instance.Id, CodeLocation.AsString());
            ActivityPersistence.WorkflowPersistence.LatestActivityInstanceId = ActivityPersistence.ActivitySummary.Instance.Id;
            
            await AggregatePostponeExceptions(taskList);
        }

        private static async Task AggregatePostponeExceptions(IList<Task> taskList)
        {
            HandledRequestPostponedException outException = null;
            var current = 0;
            while (taskList.Count > current)
            {
                try
                {
                    await taskList[current];
                    current++;
                }
                catch (HandledRequestPostponedException e)
                {
                    outException ??= new HandledRequestPostponedException();
                    outException.AddWaitingForIds(e.WaitingForRequestIds);
                    if (!outException.TryAgain) outException.TryAgain = e.TryAgain;
                    taskList.RemoveAt(current);
                }
                catch (Exception)
                {
                    current++;
                }
            }

            if (outException != null) throw outException;
            await Task.WhenAll(taskList);
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

    public class ActivityForEachParallel<TActivityReturns, TItem, TKey> : Activity
    {
        private readonly Func<CancellationToken, Task<TActivityReturns>> _getDefaultValueMethodAsync;
        public IEnumerable<TItem> Items { get; }

        public Func<TItem, TKey> GetKeyMethod;

        public object Result { get; set; }

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
                GetKeyMethod = item => (TKey) (object) item;
            }
        }

        public ActivityForEachParallel<TActivityReturns, TItem, TKey> SetGetKeyMethod(Func<TItem, TKey> method)
        {
            GetKeyMethod = method;
            return this;
        }

        public Task<IDictionary<TKey, TActivityReturns>> ExecuteAsync(
            Func<TItem, ActivityForEachParallel<TActivityReturns, TItem, TKey>, CancellationToken, Task<TActivityReturns>> method,
            CancellationToken cancellationToken = default)
        {
            return InternalExecuteAsync(
                (a, ct) => ForEachMethod(method, a, ct),
                (ct) =>  null, cancellationToken);
        }

        private Task<IDictionary<TKey, TActivityReturns>> ForEachMethod(Func<TItem,
                ActivityForEachParallel<TActivityReturns, TItem, TKey>, CancellationToken, Task<TActivityReturns>> method, 
            Activity activity, CancellationToken cancellationToken)
        {
            InternalContract.Require(GetKeyMethod != null, $"You must call {nameof(SetGetKeyMethod)} before you call the {nameof(ExecuteAsync)} method.");
            FulcrumAssert.IsNotNull(ActivityPersistence.ActivitySummary.Instance.Id, CodeLocation.AsString());
            AsyncWorkflowStatic.Context.ParentActivityInstanceId = ActivityPersistence.ActivitySummary.Instance.Id;
            var taskDictionary = new Dictionary<TKey, Task<TActivityReturns>>();
            foreach (var item in Items)
            {
                Iteration++;
                TKey key = default;
                try
                {
                    key = GetKeyMethod!(item);
                }
                catch (Exception e)
                {
                    InternalContract.Require(false, $"The {nameof(GetKeyMethod)} method failed. You must make it safe, so that it never fails.");
                }
                InternalContract.Require(key != null, $"The {nameof(GetKeyMethod)} method must not return null.");
                FulcrumAssert.IsNotNull(ActivityPersistence.ActivitySummary.Instance.Id, CodeLocation.AsString());
                ActivityPersistence.WorkflowPersistence.LatestActivityInstanceId = ActivityPersistence.ActivitySummary.Instance.Id;
                var task = MapMethodAsync(item, method, activity, cancellationToken);
                taskDictionary.Add(key!, task);
            }
            FulcrumAssert.IsNotNull(ActivityPersistence.ActivitySummary.Instance.Id, CodeLocation.AsString());
            ActivityPersistence.WorkflowPersistence.LatestActivityInstanceId = ActivityPersistence.ActivitySummary.Instance.Id;
            return AggregatePostponeExceptions(taskDictionary);
        }

        private Task<TActivityReturns> MapMethodAsync(
            TItem item,
            Func<TItem, ActivityForEachParallel<TActivityReturns, TItem, TKey>, CancellationToken, Task<TActivityReturns>> method,
            Activity instance, CancellationToken cancellationToken)
        {
            var loop = instance as ActivityForEachParallel<TActivityReturns, TItem, TKey>;
            FulcrumAssert.IsNotNull(loop, CodeLocation.AsString());
            return method(item, loop, cancellationToken);
        }

        private static async Task<IDictionary<TKey, TActivityReturns>> AggregatePostponeExceptions(IDictionary<TKey, Task<TActivityReturns>> taskDictionary)
        {
            HandledRequestPostponedException outException = null;
            var resultDictionary = new Dictionary<TKey, TActivityReturns>();
            foreach (var (key, task) in taskDictionary)
            {
                
                try
                {
                    var result = await task;
                    resultDictionary.Add(key, result);
                }
                catch (HandledRequestPostponedException e)
                {
                    outException ??= new HandledRequestPostponedException();
                    outException.AddWaitingForIds(e.WaitingForRequestIds);
                    if (!outException.TryAgain) outException.TryAgain = e.TryAgain;
                }
                catch (Exception)
                {
                    // TODO: Do we need to handle this in some way?
                }
            }

            if (outException != null) throw outException;
            return resultDictionary;
        }
    }
}