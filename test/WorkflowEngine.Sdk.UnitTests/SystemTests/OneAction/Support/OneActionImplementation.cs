using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.WorkflowEngine.Sdk;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;

namespace WorkflowEngine.Sdk.UnitTests.SystemTests.OneAction.Support;


public class OneActionImplementation : WorkflowImplementation<string>
{
    private readonly IWorkflowContainer _workflowContainer;
    private readonly IMyLogic _logic;

    /// <inheritdoc />
    public OneActionImplementation(IWorkflowContainer workflowContainer, IMyLogic logic) 
        : base(1, 0, workflowContainer)
    {
        _workflowContainer = workflowContainer;
        _logic = logic;
        DefineParameter<int>(MyWorkflowContainer.ParameterNames.ParameterA);
        if (FulcrumApplication.IsInDevelopment)
        {
            SetDebugMode();
        }
    }

    /// <inheritdoc />
    public override string GetInstanceTitle() => "InstanceTitle";

    /// <inheritdoc />
    public override IWorkflowImplementation<string> CreateWorkflowInstance()
    {
        return new OneActionImplementation(_workflowContainer, _logic);
    }

    /// <inheritdoc />
    public override async Task<string> ExecuteWorkflowAsync(CancellationToken cancellationToken = default)
    {
        var parameterA = GetWorkflowArgument<int>(MyWorkflowContainer.ParameterNames.ParameterA);
        var result = await CreateActivity<string>(1, MyWorkflowContainer.Activities.ActionA)
            .Action(a => _logic.ActionA(a, parameterA))
            .ExecuteAsync(cancellationToken);
        return result;
    }
}

public class MyWorkflowContainer : WorkflowContainer
{
    /// <inheritdoc />
    public MyWorkflowContainer(IWorkflowEngineRequiredCapabilities workflowCapabilities)
        : base("CapabilityName", "WorkflowTitle", "056AAB43-934D-4E8B-94A0-474B1F7F3811", workflowCapabilities)
    {
        DefineActivity(Activities.ActionA,nameof(Activities.ActionA), ActivityTypeEnum.Action);
    }

    /// <summary>
    /// Constants for the activities
    /// </summary>
    public static class Activities
    {
        public const string ActionA = "F5B5D5F1-577E-4014-BCF0-FBF483DF9E6A";
    }

    /// <summary>
    /// Constants for the activities
    /// </summary>
    public static class ParameterNames
    {
        public const string ParameterA = nameof(ParameterA);
    }
}

public interface IMyLogic
{
    string ActionA(IActivityAction<string> activity, int parameterA);
}