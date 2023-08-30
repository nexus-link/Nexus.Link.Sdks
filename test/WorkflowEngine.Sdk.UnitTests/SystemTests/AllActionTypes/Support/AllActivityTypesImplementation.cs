using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.WorkflowEngine.Sdk;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Activities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Execution;

namespace WorkflowEngine.Sdk.UnitTests.SystemTests.AllActionTypes.Support;


public class AllActivityTypesImplementation : WorkflowImplementation<int>
{
    private readonly IWorkflowContainer _workflowContainer;
    private readonly IAllActivityTypesLogic _logic;

    /// <inheritdoc />
    public AllActivityTypesImplementation(IWorkflowContainer workflowContainer, IAllActivityTypesLogic logic)
        : base(1, 0, workflowContainer)
    {
        _workflowContainer = workflowContainer;
        _logic = logic;
        DefineParameter<int>(AllActivityTypesContainer.ParameterNames.ParameterA);
        if (FulcrumApplication.IsInDevelopment)
        {
            SetDebugMode();
        }
    }

    /// <inheritdoc />
    public override string GetInstanceTitle() => "InstanceTitle";

    /// <inheritdoc />
    public override IWorkflowImplementation<int> CreateWorkflowInstance()
    {
        return new AllActivityTypesImplementation(_workflowContainer, _logic);
    }

    /// <inheritdoc />
    public override async Task<int> ExecuteWorkflowAsync(CancellationToken cancellationToken = default)
    {
        var parameterA = GetWorkflowArgument<int>(AllActivityTypesContainer.ParameterNames.ParameterA);

        await CreateActivity(1, AllActivityTypesContainer.Activities.Action)
            .Action((_, _) => _logic.ActionAsync())
            .ExecuteAsync(cancellationToken);

        await CreateActivity(2, AllActivityTypesContainer.Activities.If)
            .If(_logic.IfValue)
            .Then((_, _) => _logic.IfThenAsync())
            .Else((_, _) => _logic.IfElseAsync())
            .ExecuteAsync(cancellationToken);

        await CreateActivity(3, AllActivityTypesContainer.Activities.Switch)
            .Switch(_logic.SwitchValue)
            .Case(1, (_, _) => _logic.SwitchValue1Async())
            .Case(2, (_, _) => _logic.SwitchValue2Async())
            .ExecuteAsync(cancellationToken);

        var items = new List<int>();
        for (var i = 0; i < parameterA; i++)
        {
            items.Add(i + 1);
        }
        await CreateActivity(4, AllActivityTypesContainer.Activities.ForEachParallel)
            .ForEachParallel(items, (item, _, _) => _logic.ForEachParallelAsync(item))
            .ExecuteAsync(cancellationToken);

        await CreateActivity(5, AllActivityTypesContainer.Activities.ForEachSequential)
            .ForEachSequential(items, (item, _, _) => _logic.ForEachSequentialAsync(item))
            .ExecuteAsync(cancellationToken);

        await CreateActivity(6, AllActivityTypesContainer.Activities.ActionWithThrottle)
            .Action((_, _) => _logic.ActionWithThrottleAsync())
            .WithThrottle("throttle-resource-id", 1)
            .ExecuteAsync(cancellationToken);

        await CreateActivity(7, AllActivityTypesContainer.Activities.ActionUnderLock)
            .Action((_, _) => _logic.ActionUnderLockAsync())
            .UnderLock("lock-resource-id")
            .WhenWaiting(_ => _logic.ActionUnderLockAlreadyLocked())
            .ExecuteAsync(cancellationToken);

        await CreateActivity(8, AllActivityTypesContainer.Activities.Parallel)
            .Parallel()
            .AddJob(1, (_, _) => _logic.ParallelJob1Async())
            .AddJob(2, (_, _) => _logic.ParallelJob2Async())
            .ExecuteAsync(cancellationToken);

        await CreateActivity(9, AllActivityTypesContainer.Activities.Sleep)
            .Sleep(TimeSpan.Zero)
            .ExecuteAsync(cancellationToken);

        await CreateActivity(10, AllActivityTypesContainer.Activities.DoWhileOrUntil)
            .Do((a, _) => _logic.DoUntilAsync(a))
            .Until(a => a.TryGetContext<bool>(_logic.DoUntilDoneName, out var done) && done)
            .ExecuteAsync(cancellationToken);

        await CreateActivity(11, AllActivityTypesContainer.Activities.WhileDo)
            .While(a => !a.TryGetContext<bool>(_logic.WhileIncompleteName, out var incomplete) || incomplete)
            .Do((a, _) => _logic.WhileDoAsync(a))
            .ExecuteAsync(cancellationToken);

        return parameterA;
    }
}

public class AllActivityTypesContainer : WorkflowContainer
{
    /// <inheritdoc />
    public AllActivityTypesContainer(IWorkflowEngineRequiredCapabilities workflowCapabilities)
        : base("CapabilityName", "WorkflowTitle", "00ADE562-A0FE-462D-9546-6448510F6270", workflowCapabilities)
    {
        DefineActivity(Activities.Action, nameof(Activities.Action), ActivityTypeEnum.Action);
        DefineActivity(Activities.If, nameof(Activities.If), ActivityTypeEnum.If);
        DefineActivity(Activities.Switch, nameof(Activities.Switch), ActivityTypeEnum.Switch);
        DefineActivity(Activities.ForEachParallel, nameof(Activities.ForEachParallel), ActivityTypeEnum.ForEachParallel);
        DefineActivity(Activities.ForEachSequential, nameof(Activities.ForEachSequential), ActivityTypeEnum.ForEachSequential);
        DefineActivity(Activities.ActionWithThrottle, nameof(Activities.ActionWithThrottle), ActivityTypeEnum.Action);
        DefineActivity(Activities.ActionUnderLock, nameof(Activities.ActionUnderLock), ActivityTypeEnum.Action);
        DefineActivity(Activities.Parallel, nameof(Activities.Parallel), ActivityTypeEnum.Parallel);
        DefineActivity(Activities.Sleep, nameof(Activities.Sleep), ActivityTypeEnum.Sleep);
        DefineActivity(Activities.DoWhileOrUntil, nameof(Activities.DoWhileOrUntil), ActivityTypeEnum.DoWhileOrUntil);
        DefineActivity(Activities.WhileDo, nameof(Activities.WhileDo), ActivityTypeEnum.WhileDo);
    }

    /// <summary>
    /// Constants for the activities
    /// </summary>
    public static class Activities
    {
        public const string Action = "F5B5D5F1-577E-4014-BCF0-FBF483DF9E6A";
        public const string If = "1B7C9395-D692-46A1-94D2-633558C1D0ED";
        public const string Switch = "721A815B-D5AB-4937-B802-C3134C5C5C26";
        public const string ForEachParallel = "51CADD36-CDFA-4AB8-8457-04485560D84B";
        public const string ForEachSequential = "33ACF9D6-0A5E-4721-918F-DFE4E69139AC";
        public const string ActionWithThrottle = "5281B800-08E0-417D-A3D9-0322CE77793B";
        public const string ActionUnderLock = "69DF0430-6B06-4240-9913-0647E58E4EF2";
        public const string Parallel = "AC967BA1-572B-4D91-81F2-0A6AA83BE984";
        public const string Sleep = "D2FDD0E6-5995-4439-9266-03DAC7CC972B";
        public const string DoWhileOrUntil = "B88BD710-CD85-4616-9E20-27AF58E858E9";
        public const string WhileDo = "C2574EBC-D60B-428E-A1BE-AA46C87F5681";
    }

    /// <summary>
    /// Constants for the activities
    /// </summary>
    public static class ParameterNames
    {
        public const string ParameterA = nameof(ParameterA);
    }
}

public interface IAllActivityTypesLogic
{
    Task ActionAsync();
    bool IfValue { get; }
    Task IfThenAsync();
    Task IfElseAsync();
    int SwitchValue { get; }
    string WhileIncompleteName { get; }
    string DoUntilDoneName { get; }
    Task SwitchValue1Async();
    Task SwitchValue2Async();
    Task ForEachParallelAsync(int item);
    Task ForEachSequentialAsync(int item);
    Task ActionWithThrottleAsync();
    Task ActionUnderLockAsync();
    Task ParallelJob1Async();
    Task ParallelJob2Async();
    Task DoUntilAsync(IActivityDoWhileOrUntil activityWhileDo);
    Task WhileDoAsync(IActivityWhileDo activityWhileDo);
    void ActionUnderLockAlreadyLocked();
}