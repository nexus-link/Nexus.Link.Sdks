using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.WorkflowEngine.Sdk;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Execution;

namespace WorkflowEngine.Sdk.UnitTests.TestSupport;

public class TestWorkflowImplementation : WorkflowImplementation
{
    protected readonly IWorkflowEngineRequiredCapabilities WorkflowCapabilities;
    private readonly Func<CancellationToken, Task> _onExecuteAsync;

    public TestWorkflowImplementation(IWorkflowEngineRequiredCapabilities workflowCapabilities, Func<CancellationToken, Task> onExecuteAsync = null)
        : base(1, 2, new TestWorkflowContainer(workflowCapabilities))
    {
        WorkflowCapabilities = workflowCapabilities;
        _onExecuteAsync = onExecuteAsync;
    }

    /// <inheritdoc />
    public override string GetInstanceTitle() => "Workflow instance title";

    /// <inheritdoc />
    public override IWorkflowImplementation CreateWorkflowInstance()
    {
        throw new System.NotImplementedException();
    }

    /// <inheritdoc />
    public override async Task ExecuteWorkflowAsync(CancellationToken cancellationToken = default)
    {
        if (_onExecuteAsync != null)
        {
            await _onExecuteAsync(cancellationToken);
        }
        else
        {
            throw new System.NotImplementedException();
        }
    }
}
public class TestWorkflowImplementationR : WorkflowImplementation<int>
{
    protected readonly IWorkflowEngineRequiredCapabilities WorkflowCapabilities;
    private readonly Func<CancellationToken, Task> _onExecuteAsync;

    public TestWorkflowImplementationR(IWorkflowEngineRequiredCapabilities workflowCapabilities, Func<CancellationToken, Task> onExecuteAsync = null)
        : base(1, 2, new TestWorkflowContainer(workflowCapabilities))
    {
        WorkflowCapabilities = workflowCapabilities;
        _onExecuteAsync = onExecuteAsync;
    }

    /// <inheritdoc />
    public override string GetInstanceTitle() => "Workflow instance title";

    /// <inheritdoc />
    public override IWorkflowImplementation<int> CreateWorkflowInstance()
    {
        throw new System.NotImplementedException();
    }

    /// <inheritdoc />
    public override async Task<int> ExecuteWorkflowAsync(CancellationToken cancellationToken = default)
    {
        if (_onExecuteAsync != null)
        {
            await _onExecuteAsync(cancellationToken);
            return 0;
        }
        throw new System.NotImplementedException();
    }
}