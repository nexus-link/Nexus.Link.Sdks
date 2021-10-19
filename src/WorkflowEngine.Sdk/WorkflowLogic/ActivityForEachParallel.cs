using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.AsyncManager.Sdk;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Exceptions;
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

        public async Task ExecuteAsync(
            Func<TItemType, ActivityForEachParallel<TItemType>, CancellationToken, Task> method,
            CancellationToken cancellationToken, params object[] arguments)
        {
            var taskList = new List<Task>();
            foreach (var item in Items)
            {
                var task = InternalExecuteAsync((instance, ct) => MapMethod(item, method, instance, ct),
                    cancellationToken);
                taskList.Add(task);
            }
            
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

    public class ActivityForEachParallel<TActivityReturns, TItemType> : Activity
    {
        private readonly Func<Task<TActivityReturns>> _getDefaultValueMethodAsync;
        public IEnumerable<TItemType> Items { get; }

        public object Result { get; set; }

        public ActivityForEachParallel(ActivityInformation activityInformation,
            IAsyncRequestClient asyncRequestClient, IEnumerable<TItemType> items,
            Activity previousActivity, Activity parentActivity, Func<Task<TActivityReturns>> getDefaultValueMethodAsync)
            : base(activityInformation, asyncRequestClient, previousActivity, parentActivity)
        {
            _getDefaultValueMethodAsync = getDefaultValueMethodAsync;
            Items = items;
            InternalContract.RequireAreEqual(WorkflowActivityTypeEnum.ForEachParallel, ActivityInformation.ActivityType, "Ignore",
                $"The activity {ActivityInformation} was declared as {ActivityInformation.ActivityType}, so you can't use {nameof(ActivityForEachParallel<TItemType>)}.");
        }

        public async Task<List<Task<TActivityReturns>>> ExecuteAsync(
            Func<TItemType, ActivityForEachParallel<TActivityReturns, TItemType>, CancellationToken, Task<TActivityReturns>> method,
            CancellationToken cancellationToken, params object[] arguments)
        {
            var taskList = new List<Task<TActivityReturns>>();
            foreach (var item in Items)
            {
                var task = InternalExecuteAsync((instance, ct) => MapMethod(item, method, instance, ct),
                    _getDefaultValueMethodAsync, cancellationToken);
                taskList.Add(task);
            }
            return await AggregatePostponeExceptions(taskList);
        }

        private static async Task<List<Task<TActivityReturns>>> AggregatePostponeExceptions(List<Task<TActivityReturns>> taskList)
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
                    taskList.RemoveAt(current);
                }
                catch (Exception)
                {
                    current++;
                }
            }

            if (outException != null) throw outException;
            await Task.WhenAll(taskList);
            return taskList;
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