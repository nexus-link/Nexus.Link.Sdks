using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.AsyncManager.Sdk;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Model;

namespace Nexus.Link.WorkflowEngine.Sdk.WorkflowLogic
{
    public abstract class ActivityLoopUntilTrueBase : Activity
    {
        private readonly Dictionary<string, object> _loopArguments = new Dictionary<string, object>();

        public bool? EndLoop { get; set; }

        public ActivityLoopUntilTrueBase(ActivityInformation activityInformation, IAsyncRequestClient asyncRequestClient,
            Activity previousActivity, Activity parentActivity)
            : base(activityInformation, asyncRequestClient, previousActivity, parentActivity)
        {
            InternalContract.RequireAreEqual(WorkflowActivityTypeEnum.LoopUntilTrue, ActivityInformation.ActivityType, "Ignore",
                $"The activity {ActivityInformation} was declared as {ActivityInformation.ActivityType}, so you can't use {nameof(ActivityLoopUntilTrue)}.");
            Iteration = 0;
        }

        public void SetLoopArgument<T>(string name, T value)
        {
            _loopArguments[name] = value;
        }

        public T GetLoopArgument<T>(string name)
        {
            if (!_loopArguments.ContainsKey(name)) return default;
            return (T)_loopArguments[name];
        }
    }
    
    public class ActivityLoopUntilTrue : ActivityLoopUntilTrueBase
    {
        public ActivityLoopUntilTrue(ActivityInformation activityInformation, IAsyncRequestClient asyncRequestClient,
            Activity previousActivity, Activity parentActivity)
            : base(activityInformation, asyncRequestClient, previousActivity, parentActivity)
        {
        }

        public async Task ExecuteAsync(
            Func<ActivityLoopUntilTrue, CancellationToken, Task> method,
            CancellationToken cancellationToken, params object[] arguments)
        {
            EndLoop = null;
            do
            {
                Iteration++;
                // TODO: Verify that we don't use the same values each iteration
                await InternalExecuteAsync((instance, ct) => MapMethod(method, instance, ct), cancellationToken);
                InternalContract.RequireNotNull(EndLoop, "ignore", $"You must set {nameof(EndLoop)} before returning.");
            } while (EndLoop != true);
        }

        private Task MapMethod(
            Func<ActivityLoopUntilTrue, CancellationToken, Task> method,
            Activity instance, CancellationToken cancellationToken)
        {
            var loop = instance as ActivityLoopUntilTrue;
            FulcrumAssert.IsNotNull(loop, CodeLocation.AsString());
            return method(loop, cancellationToken);
        }
    }
    public class ActivityLoopUntilTrue<TActivityReturns> : ActivityLoopUntilTrueBase
    {
        public ActivityLoopUntilTrue(ActivityInformation activityInformation, IAsyncRequestClient asyncRequestClient,
            Activity previousActivity, Activity parentActivity)
            : base(activityInformation, asyncRequestClient, previousActivity, parentActivity)
        {
        }

        public async Task<TActivityReturns> ExecuteAsync(
            Func<ActivityLoopUntilTrue, CancellationToken, Task<TActivityReturns>> method,
            CancellationToken cancellationToken)
        {
            EndLoop = null;
            TActivityReturns result;
            do
            {
                Iteration++;
                // TODO: Verify that we don't use the same values each iteration
                result = await InternalExecuteAsync((instance, ct) => MapMethod(method, instance, ct), cancellationToken);
                InternalContract.RequireNotNull(EndLoop, "ignore", $"You must set {nameof(EndLoop)} before returning.");
            } while (EndLoop != true);

            return result;
        }

        private Task<TMethodReturnType> MapMethod<TMethodReturnType>(
            Func<ActivityLoopUntilTrue, CancellationToken, Task<TMethodReturnType>> method,
            Activity instance, CancellationToken cancellationToken)
        {
            var loop = instance as ActivityLoopUntilTrue;
            FulcrumAssert.IsNotNull(loop, CodeLocation.AsString());
            return method(loop, cancellationToken);
        }
    }
}