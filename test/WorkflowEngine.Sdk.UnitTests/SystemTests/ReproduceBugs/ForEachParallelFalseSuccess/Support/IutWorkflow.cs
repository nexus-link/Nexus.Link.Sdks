using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.WorkflowEngine.Sdk;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Activities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Execution;

namespace WorkflowEngine.Sdk.UnitTests.SystemTests.ReproduceBugs.ForEachParallelFalseSuccess.Support;


public class IutWorkflowImplementation : WorkflowImplementation<int>
{
    private const string ContextResponses = "Sum";
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
    public override IWorkflowImplementation<int> CreateWorkflowInstance()
    {
        return new IutWorkflowImplementation(_workflowContainer, _logic);
    }

    /// <inheritdoc />
    public override async Task<int> ExecuteWorkflowAsync(CancellationToken cancellationToken = default)
    {
        var items = new List<int>();
        for (var i = 0; i < 2; i++)
        {
            items.Add(i + 1);
        }
        var forEachActivity = CreateActivity(1, IutWorkflowContainer.Activities.ForEachParallel)
            .ForEachParallel(items, IndividualActivityAsync);
        var responses = new ConcurrentDictionary<int, int>
        {
            [0] = 0
        };
        forEachActivity.SetContext(ContextResponses, responses);
        await forEachActivity
            .ExecuteAsync(cancellationToken);
        responses = forEachActivity.GetContext<ConcurrentDictionary<int, int>>(ContextResponses);
        return responses.Values.Aggregate((sum, number) => sum + number);
    }

    private async Task IndividualActivityAsync(int item, IActivityForEachParallel<int> forEachActivity, CancellationToken cancellationToken)
    {
        var responses = forEachActivity.GetContext<ConcurrentDictionary<int, int>>(ContextResponses);
        var ifActivity = CreateActivity(1, IutWorkflowContainer.Activities.If)
            .If(item == 1)
            .Then(ThenActivityAsync)
            .Else(ElseActivityAsync);
        await ifActivity
            .ExecuteAsync(cancellationToken);
        responses.TryAdd(forEachActivity.LoopIteration, 1);
        forEachActivity.SetContext(ContextResponses, responses);
    }

    private async Task ElseActivityAsync(IActivityIf activity, CancellationToken cancellationToken)
    {
        await CreateActivity(1, IutWorkflowContainer.Activities.Action1)
            .Action((a, _) => _logic.Action1Async(a))
            .ExecuteAsync(cancellationToken);
    }

    private async Task ThenActivityAsync(IActivityIf activity, CancellationToken cancellationToken)
    {
        await CreateActivity(1, IutWorkflowContainer.Activities.Action2)
            .Action((a, _) => _logic.Action2Async(a))
            .ExecuteAsync(cancellationToken);
    }
}

public class IutWorkflowContainer : WorkflowContainer
{
    /// <inheritdoc />
    public IutWorkflowContainer(IWorkflowEngineRequiredCapabilities workflowCapabilities)
        : base("CapabilityName", "WorkflowTitle", "F72DEE33-2D14-4763-95D4-6B517180D328", workflowCapabilities)
    {
        DefineActivity(Activities.ForEachParallel, nameof(Activities.ForEachParallel), ActivityTypeEnum.ForEachParallel);
        DefineActivity(Activities.If, nameof(Activities.If), ActivityTypeEnum.If);
        DefineActivity(Activities.Action1, nameof(Activities.Action1), ActivityTypeEnum.Action);
        DefineActivity(Activities.Action2, nameof(Activities.Action2), ActivityTypeEnum.Action);
    }

    /// <summary>
    /// Constants for the activities
    /// </summary>
    public static class Activities
    {
        public const string ForEachParallel = "51CADD36-CDFA-4AB8-8457-04485560D84B";
        public const string If = "7E32D311-0509-4D13-98AF-DE256CB946C7";
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