using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.UnitTest.Exceptions;

namespace Nexus.Link.WorkflowEngine.Sdk.UnitTest;

public abstract class WorkflowFastForward : WorkflowImplementation
{
    protected bool HasSetContext { get; private set; }
    protected int? BreakAtIteration { get; private set; }
    protected string BreakAtActivityFormId { get; private set; }

    protected WorkflowFastForward(int majorVersion, int minorVersion, IWorkflowContainer workflowContainer) : base(majorVersion, minorVersion, workflowContainer)
    {
    }

    public override string GetInstanceTitle() => "Workflow instance title";

    public override IWorkflowImplementation CreateWorkflowInstance()
    {
        throw new NotImplementedException();
    }

    public IWorkflowImplementation SetBreakCondition(string activityFormId, int? iteration = null)
    {
        BreakAtActivityFormId = activityFormId;
        BreakAtIteration = iteration;
        HasSetContext = true;
        return this;
    }

    public void MaybeBreak(IActivityLoopUntilTrueBase activity)
    {
        FulcrumAssert.IsNotNull(activity.Iteration, CodeLocation.AsString());
        MaybeBreak(activity.ActivityFormId, activity.Iteration!.Value);
    }

    private void MaybeBreak(string activityFormId, int iteration)
    {
        if (!BreakAtIteration.HasValue)
        {
            throw new WorkflowUnitTestFailedException(
                $"Break condition for activity {activityFormId} must include an iteration value.");
        }
        if (string.Equals(BreakAtActivityFormId, activityFormId, StringComparison.InvariantCultureIgnoreCase))
        {
            if (BreakAtIteration.Value != iteration) return;
            throw new ExceptionTransporter(new WorkflowFastForwardBreakException());
        }
    }

    /// <inheritdoc />
    public override async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        InternalContract.Require(HasSetContext, $"You must call {nameof(SetBreakCondition)}.");
        try
        {
            await base.ExecuteAsync(cancellationToken);
            if (BreakAtActivityFormId != null)
            {

                var message = $"The {this.GetType().Name} fast forward class didn't specify the activity {BreakAtActivityFormId}";
                if (BreakAtIteration.HasValue)
                {
                    message += $" with iteration {BreakAtIteration.Value}";
                }
                throw new WorkflowUnitTestFailedException($"{message}.");
            }
        }
        catch (WorkflowFastForwardBreakException)
        {
            // This is OK
        }
    }

    public new IActivityFlow CreateActivity(int position, string activityFormId)
    {
        MaybeBreak(activityFormId);
        return base.CreateActivity(position, activityFormId);
    }

    public new IActivityFlow<T> CreateActivity<T>(int position, string activityFormId)
    {
        MaybeBreak(activityFormId);
        return base.CreateActivity<T>(position, activityFormId);
    }

    private void MaybeBreak(string activityFormId)
    {
        var activity = WorkflowContainer.GetActivityDefinition(activityFormId);
        FulcrumAssert.IsNotNull(activity);
        
        // Don't break at this point for loops
        switch (activity.Type)
        {
            case ActivityTypeEnum.Action:
            case ActivityTypeEnum.Condition:
            case ActivityTypeEnum.ForEachParallel:
            case ActivityTypeEnum.ForEachSequential:
            case ActivityTypeEnum.Semaphore:
                break;
            case ActivityTypeEnum.LoopUntilTrue:
                return;
            default:
                FulcrumAssert.Fail(CodeLocation.AsString());
                break;
        }
        if (string.Equals(BreakAtActivityFormId, activityFormId, StringComparison.InvariantCultureIgnoreCase))
        {
            if (BreakAtIteration.HasValue)
            {
                var parentActivity = CurrentParentActivity;
                if (parentActivity == null)
                {
                    throw new WorkflowUnitTestFailedException(
                        $"Break condition for activity {activityFormId} included iteration {BreakAtIteration.Value}, so the activity was expected to have a parent activity.");
                }

                if (!parentActivity.Iteration.HasValue)
                {
                    throw new WorkflowUnitTestFailedException(
                        $"Break condition for activity {activityFormId} included iteration {BreakAtIteration.Value}, so the parent activity was expected to have an iteration value.");
                }

                if (BreakAtIteration.Value != parentActivity.Iteration.Value) return;
            }

            throw new ExceptionTransporter(new WorkflowFastForwardBreakException());
        }
    }
}