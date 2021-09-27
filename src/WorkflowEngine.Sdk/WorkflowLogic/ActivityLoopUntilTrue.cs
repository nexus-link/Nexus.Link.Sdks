using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;
using WorkflowEngine.Sdk.Model;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;

namespace WorkflowEngine.Sdk.WorkflowLogic
{
    public class ActivityLoopUntilTrue : Activity
    {
        public readonly Dictionary<int, string> IterationDescriptions = new Dictionary<int, string>();
        public bool? EndLoop { get; set; }

        public ActivityLoopUntilTrue(IWorkflowCapabilityForClient workflowCapability,
            ActivityInformation activityInformation, 
            Activity previousActivity, Activity parentActivity)
            : base(workflowCapability, activityInformation, previousActivity, parentActivity)
        {
            InternalContract.RequireAreEqual(WorkflowActivityTypeEnum.LoopUntilTrue, ActivityInformation.ActivityType, "Ignore",
                $"The activity {ActivityInformation} was declared as {ActivityInformation.ActivityType}, so you can't use {nameof(ActivityLoopUntilTrue)}.");
        }

        public async Task<TMethodReturnType> ExecuteAsync<TMethodReturnType>(
            Func<ActivityLoopUntilTrue, CancellationToken, Task<TMethodReturnType>> method,
            CancellationToken cancellationToken, params object[] arguments)
        {
            InternalContract.Require(
                ActivityInformation.ActivityType == WorkflowActivityTypeEnum.Action ||
                ActivityInformation.ActivityType == WorkflowActivityTypeEnum.LoopUntilTrue,
                $"The activity {ActivityInformation} was declared as {ActivityInformation.ActivityType}, so you can't call {nameof(ExecuteAsync)}.");

            EndLoop = null;
            TMethodReturnType result;
            do
            {
                Iteration++;
                // TODO: Verify that we don't use the same values each iteration
                result = await InternalExecuteAsync((instance, ct) => MapMethod(method, instance, ct),
                    cancellationToken);
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

        public async Task ExecuteAsync(
            Func<ActivityLoopUntilTrue, CancellationToken, Task<bool>> method,
            CancellationToken cancellationToken, params object[] arguments)
        {
            InternalContract.Require(
                ActivityInformation.ActivityType == WorkflowActivityTypeEnum.Action ||
                ActivityInformation.ActivityType == WorkflowActivityTypeEnum.LoopUntilTrue,
                $"The activity {ActivityInformation} was declared as {ActivityInformation.ActivityType}, so you can't call {nameof(ExecuteAsync)}.");


            var done = false;
            while (!done)
            {
                done = await InternalExecuteAsync((instance, ct) => MapMethod(method, instance, ct), cancellationToken);
            }
        }

        private Task MapMethod(
            Func<ActivityLoopUntilTrue, CancellationToken, Task> method,
            Activity instance, CancellationToken cancellationToken)
        {
            var loop = instance as ActivityLoopUntilTrue;
            FulcrumAssert.IsNotNull(loop, CodeLocation.AsString());
            return method(loop, cancellationToken);
        }

        public Task IterationAsync(string description)
        {
            Iteration++;
            IterationDescriptions[Iteration] = description;
            return Task.CompletedTask;
        }

        public override string IdentifierIndex
        {
            get
            {
                if (Iteration == 0) throw new FulcrumContractException($"A loop must have a call to {nameof(IterationAsync)} before any activities are executed.");
                return $"({Iteration})";
            }
        }

        private readonly Dictionary<string, object> _loopArguments = new Dictionary<string, object>();

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
}