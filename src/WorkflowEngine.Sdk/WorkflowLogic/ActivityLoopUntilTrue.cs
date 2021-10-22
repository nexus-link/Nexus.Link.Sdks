using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Support;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Model;

namespace Nexus.Link.WorkflowEngine.Sdk.WorkflowLogic
{
    public abstract class ActivityLoopUntilTrueBase : Activity
    {
        private readonly Dictionary<string, object> _loopArguments = new();

        public bool? EndLoop { get; set; }

        protected ActivityLoopUntilTrueBase(ActivityInformation activityInformation, IActivityExecutor activityExecutor)
            : base(activityInformation, activityExecutor)
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
        public ActivityLoopUntilTrue(ActivityInformation activityInformation, IActivityExecutor activityExecutor)
            : base(activityInformation, activityExecutor)
        {
        }

        public async Task ExecuteAsync(
            Func<ActivityLoopUntilTrue, CancellationToken, Task> method,
            CancellationToken cancellationToken = default)
        {
            await ActivityExecutor.ExecuteAsync(
                (a, ct) => LoopUntilMethod(method, a, ct), cancellationToken);
        }

        private async Task LoopUntilMethod(Func<ActivityLoopUntilTrue, CancellationToken, Task> method, Activity activity, CancellationToken cancellationToken)
        {
            FulcrumAssert.IsNotNull(ActivityInformation.InstanceId, CodeLocation.AsString());
            AsyncWorkflowStatic.Context.ParentActivityInstanceId = ActivityInformation.InstanceId;
            EndLoop = null;
            do
            {
                Iteration++;
                // TODO: Verify that we don't use the same values each iteration
                 await MapMethodAsync(method, activity, cancellationToken);
                InternalContract.RequireNotNull(EndLoop, "ignore", $"You must set {nameof(EndLoop)} before returning.");
                FulcrumAssert.IsNotNull(ActivityInformation.InstanceId, CodeLocation.AsString());
                ActivityInformation.WorkflowInformation.LatestActivityInstanceId = ActivityInformation.InstanceId;
            } while (EndLoop != true);
        }

        private Task MapMethodAsync(
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
        private readonly Func<CancellationToken, Task<TActivityReturns>> _getDefaultValueMethodAsync;

        public ActivityLoopUntilTrue(ActivityInformation activityInformation, IActivityExecutor activityExecutor, Func<CancellationToken, Task<TActivityReturns>> getDefaultValueMethodAsync)
            : base(activityInformation, activityExecutor)
        {
            _getDefaultValueMethodAsync = getDefaultValueMethodAsync;
        }

        public async Task<TActivityReturns> ExecuteAsync(
            Func<ActivityLoopUntilTrue<TActivityReturns>, CancellationToken, Task<TActivityReturns>> method,
            CancellationToken cancellationToken = default)
        {
            return await ActivityExecutor.ExecuteAsync(
                (a, ct) => LoopUntilMethod(method, a, ct),
                _getDefaultValueMethodAsync, cancellationToken);
        }

        private async Task<TActivityReturns> LoopUntilMethod(Func<ActivityLoopUntilTrue<TActivityReturns>, CancellationToken, Task<TActivityReturns>> method, Activity activity, CancellationToken cancellationToken)
        {
            FulcrumAssert.IsNotNull(ActivityInformation.InstanceId, CodeLocation.AsString());
            AsyncWorkflowStatic.Context.ParentActivityInstanceId = ActivityInformation.InstanceId;
            EndLoop = null;
            TActivityReturns result;
            do
            {
                Iteration++;
                // TODO: Verify that we don't use the same values each iteration
                result = await MapMethodAsync(method, activity, cancellationToken);
                InternalContract.RequireNotNull(EndLoop, "ignore", $"You must set {nameof(EndLoop)} before returning.");
                FulcrumAssert.IsNotNull(ActivityInformation.InstanceId, CodeLocation.AsString());
                ActivityInformation.WorkflowInformation.LatestActivityInstanceId = ActivityInformation.InstanceId;
            } while (EndLoop != true);

            return result;
        }

        private Task<TActivityReturns> MapMethodAsync(
            Func<ActivityLoopUntilTrue<TActivityReturns>, CancellationToken, Task<TActivityReturns>> method,
            Activity instance, CancellationToken cancellationToken)
        {
            var loop = instance as ActivityLoopUntilTrue<TActivityReturns>;
            FulcrumAssert.IsNotNull(loop, CodeLocation.AsString());
            return method(loop, cancellationToken);
        }
    }
}