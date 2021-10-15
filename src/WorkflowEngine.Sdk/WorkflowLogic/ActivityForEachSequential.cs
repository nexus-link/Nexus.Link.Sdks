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
            : base(activityInformation, asyncRequestClient, previousActivity, parentActivity)
        {
            Items = items;
            InternalContract.RequireAreEqual(WorkflowActivityTypeEnum.ForEachSequential, ActivityInformation.ActivityType, "Ignore",
                $"The activity {ActivityInformation} was declared as {ActivityInformation.ActivityType}, so you can't use {nameof(ActivityForEachParallel<TItemType>)}.");
        }

        public async Task<List<TMethodReturnType>> ExecuteAsync<TMethodReturnType>(
            Func<TItemType, ActivityForEachParallel<TItemType>, CancellationToken, Task<TMethodReturnType>> method,
            CancellationToken cancellationToken, params object[] arguments)
        {
            var resultList = new List<TMethodReturnType>();
            foreach (var item in Items)
            {
                var result = await InternalExecuteAsync((instance, ct) => MapMethod(item, method, instance, ct),
                                    cancellationToken);
                resultList.Add(result);
            }

            return resultList;
        }

        public async Task ExecuteAsync(
            Func<TItemType, ActivityForEachParallel<TItemType>, CancellationToken, Task> method,
            CancellationToken cancellationToken, params object[] arguments)
        {

            await ExecuteAsync(async (item, a, ct) =>
            {
                await method(item, a, ct);
                return true;
            }, cancellationToken);
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
    }
}