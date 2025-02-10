using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.WorkflowEngine.Sdk;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Activities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Execution;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;

namespace WorkflowEngine.Sdk.UnitTests.SystemTests.ReproduceBugs.WorkflowCancelledExceptionIgnored.Support;


public class IutWorkflowImplementation : WorkflowImplementation
{
    private readonly IWorkflowContainer _workflowContainer;
    private readonly IIutWorkflowLogic _logic;

    /// <inheritdoc />
    public IutWorkflowImplementation(IWorkflowContainer workflowContainer, IIutWorkflowLogic logic)
        : base(1, 0, workflowContainer)
    {
        _workflowContainer = workflowContainer;
        _logic = logic;
        SetDebugMode();
    }

    /// <inheritdoc />
    public override string GetInstanceTitle() => "InstanceTitle";

    /// <inheritdoc />
    public override IWorkflowImplementation CreateWorkflowInstance()
    {
        return new IutWorkflowImplementation(_workflowContainer, _logic);
    }

    /// <inheritdoc />
    public override async Task ExecuteWorkflowAsync(CancellationToken cancellationToken = default)
    {
        await CreateActivity(1, IutWorkflowContainer.Activities.ActionUnderLock)
            .Action(MainAsync)
            .UnderLock("hello")
            .ExecuteAsync(cancellationToken);
        //await CreateActivity(1, IutWorkflowContainer.Activities.Lock)
        //    .Lock("a")
        //    .Then(MainLockAsync)
        //    .ExecuteAsync(cancellationToken);
    }

    private async Task MainLockAsync(IActivityLock activity, CancellationToken cancellationToken)
    {
        await CreateActivity(1, IutWorkflowContainer.Activities.Action1)
            .Action((a, _) => _logic.Action1Async(a))
            .CatchAll((a, e) => throw new WorkflowFailedException(ActivityExceptionCategoryEnum.TechnicalError, "Technical message",
                "Friendly message"))
            .ExecuteAsync(cancellationToken);
        await CreateActivity(2, IutWorkflowContainer.Activities.Action2)
            .Action((a, _) => _logic.Action2Async(a))
            .ExecuteAsync(cancellationToken);
    }

    private async Task MainAsync(IActivityAction activity, CancellationToken cancellationToken)
    {
        await CreateActivity(1, IutWorkflowContainer.Activities.Action1)
            .Action((a, _) => _logic.Action1Async(a))
            .CatchAll((a, e) => throw new WorkflowFailedException(ActivityExceptionCategoryEnum.TechnicalError, "Technical message",
                "Friendly message"))
            .ExecuteAsync(cancellationToken);
        await CreateActivity(2, IutWorkflowContainer.Activities.Action2)
            .Action((a, _) => _logic.Action2Async(a))
            .ExecuteAsync(cancellationToken);
    }
}

public class IutWorkflowContainer : WorkflowContainer
{
    /// <inheritdoc />
    public IutWorkflowContainer(IWorkflowEngineRequiredCapabilities workflowCapabilities)
        : base("CapabilityName", "WorkflowTitle", "52875C8C-EE3B-49EF-BCA8-EC72B10D4387", workflowCapabilities)
    {
        DefineActivity(Activities.Lock, nameof(Activities.Lock), ActivityTypeEnum.Lock);
        DefineActivity(Activities.ActionUnderLock, nameof(Activities.ActionUnderLock), ActivityTypeEnum.Action);
        DefineActivity(Activities.Action1, nameof(Activities.Action1), ActivityTypeEnum.Action);
        DefineActivity(Activities.Action2, nameof(Activities.Action2), ActivityTypeEnum.Action);
    }

    /// <summary>
    /// Constants for the activities
    /// </summary>
    public static class Activities
    {
        public const string Lock = "F9EA70E8-6CD6-4C74-B089-464DFF2145D9";
        public const string ActionUnderLock = "F9705C7A-EA1B-45B5-A3B7-21DE706DC2C3";
        public const string Action1 = "F5B5D5F1-577E-4014-BCF0-FBF483DF9E6A";
        public const string Action2 = "AE93F55D-C44C-4C98-8864-EB159D734D49";
    }

    /// <summary>
    /// Constants for the activities
    /// </summary>
    public static class ParameterNames
    {
    }
}

public interface IIutWorkflowLogic
{
    Task Action1Async(IActivityAction activity);
    Task Action2Async(IActivityAction activity);
}