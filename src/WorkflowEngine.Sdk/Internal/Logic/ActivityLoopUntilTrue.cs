using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Support;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Logic
{
    internal abstract class ActivityLoopUntilTrueBase : Activity, IActivityLoopUntilTrueBase
    {
        private readonly Dictionary<string, object> _loopArguments = new();

        public bool? EndLoop { get; set; }

        protected ActivityLoopUntilTrueBase(IActivityInformation activityInformation)
            : base(activityInformation)
        {
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

    internal class ActivityLoopUntilTrue : ActivityLoopUntilTrueBase, IActivityLoopUntilTrue
    {
        public ActivityLoopUntilTrue(IActivityInformation activityInformation)
            : base(activityInformation)
        {
        }

        public async Task ExecuteAsync(
            Func<IActivityLoopUntilTrue, CancellationToken, Task> method,
            CancellationToken cancellationToken = default)
        {
            await InternalExecuteAsync(
                (a, ct) => LoopUntilMethod(method, a, ct), cancellationToken);
        }

        private async Task LoopUntilMethod(Func<IActivityLoopUntilTrue, CancellationToken, Task> method, IActivity activity, CancellationToken cancellationToken)
        {
            FulcrumAssert.IsNotNull(Instance.Id, CodeLocation.AsString());
            WorkflowStatic.Context.ParentActivityInstanceId = Instance.Id;
            EndLoop = null;
            do
            {
                Iteration++;
                // TODO: Verify that we don't use the same values each iteration
                await MapMethodAsync(method, activity, cancellationToken);
                InternalContract.RequireNotNull(EndLoop, "ignore", $"You must set {nameof(EndLoop)} before returning.");
                FulcrumAssert.IsNotNull(Instance.Id, CodeLocation.AsString());
                ActivityInformation.Workflow.LatestActivity = this;
            } while (EndLoop != true);
        }

        private Task MapMethodAsync(
            Func<IActivityLoopUntilTrue, CancellationToken, Task> method,
            IActivity instance, CancellationToken cancellationToken)
        {
            var loop = instance as IActivityLoopUntilTrue;
            FulcrumAssert.IsNotNull(loop, CodeLocation.AsString());
            return method(loop, cancellationToken);
        }
    }

    internal class ActivityLoopUntilTrue<TActivityReturns> : ActivityLoopUntilTrueBase, IActivityLoopUntilTrue<TActivityReturns>
    {
        private readonly Func<CancellationToken, Task<TActivityReturns>> _getDefaultValueMethodAsync;

        public ActivityLoopUntilTrue(IActivityInformation activityInformation, Func<CancellationToken, Task<TActivityReturns>> getDefaultValueMethodAsync)
            : base(activityInformation)
        {
            _getDefaultValueMethodAsync = getDefaultValueMethodAsync;
        }

        public async Task<TActivityReturns> ExecuteAsync(
            Func<IActivityLoopUntilTrue<TActivityReturns>, CancellationToken, Task<TActivityReturns>> method,
            CancellationToken cancellationToken = default)
        {
            return await InternalExecuteAsync(
                (a, ct) => LoopUntilMethod(method, a, ct),
                _getDefaultValueMethodAsync, cancellationToken);
        }

        private async Task<TActivityReturns> LoopUntilMethod(Func<IActivityLoopUntilTrue<TActivityReturns>, CancellationToken, Task<TActivityReturns>> method, IActivity activity, CancellationToken cancellationToken)
        {
            FulcrumAssert.IsNotNull(Instance.Id, CodeLocation.AsString());
            WorkflowStatic.Context.ParentActivityInstanceId = Instance.Id;
            EndLoop = null;
            TActivityReturns result;
            do
            {
                Iteration++;
                // TODO: Verify that we don't use the same values each iteration
                result = await MapMethodAsync(method, activity, cancellationToken);
                InternalContract.RequireNotNull(EndLoop, "ignore", $"You must set {nameof(EndLoop)} before returning.");
                FulcrumAssert.IsNotNull(Instance.Id, CodeLocation.AsString());
                ActivityInformation.Workflow.LatestActivity = this;
            } while (EndLoop != true);

            return result;
        }

        private Task<TActivityReturns> MapMethodAsync(
            Func<IActivityLoopUntilTrue<TActivityReturns>, CancellationToken, Task<TActivityReturns>> method,
            IActivity instance, CancellationToken cancellationToken)
        {
            var loop = instance as IActivityLoopUntilTrue<TActivityReturns>;
            FulcrumAssert.IsNotNull(loop, CodeLocation.AsString());
            return method(loop, cancellationToken);
        }
    }
}