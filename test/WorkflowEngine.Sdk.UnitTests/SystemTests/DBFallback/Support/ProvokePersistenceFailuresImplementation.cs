using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.WorkflowEngine.Sdk;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Execution;

namespace WorkflowEngine.Sdk.UnitTests.SystemTests.DBFallback.Support;


public class ProvokePersistenceFailuresImplementation : WorkflowImplementation<string>
{
    private readonly IWorkflowContainer _workflowContainer;
    private readonly IProvokePersistenceFailures _logic;

    /// <inheritdoc />
    public ProvokePersistenceFailuresImplementation(IWorkflowContainer workflowContainer, IProvokePersistenceFailures logic) 
        : base(1, 0, workflowContainer)
    {
        _workflowContainer = workflowContainer;
        _logic = logic;
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
        return new ProvokePersistenceFailuresImplementation(_workflowContainer, _logic);
    }

    /// <inheritdoc />
    public override async Task<string> ExecuteWorkflowAsync(CancellationToken cancellationToken = default)
    {
        var result = await CreateActivity<string>(1, ProvokePersistenceFailuresContainer.Activities.ActionA)
            .Action(a => _logic.ActionA())
            .ExecuteAsync(cancellationToken);
        return result;
    }
}

public class ProvokePersistenceFailuresContainer : WorkflowContainer
{
    /// <inheritdoc />
    public ProvokePersistenceFailuresContainer(IWorkflowEngineRequiredCapabilities workflowCapabilities)
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
}

public interface IProvokePersistenceFailures
{
    string ActionA();
}