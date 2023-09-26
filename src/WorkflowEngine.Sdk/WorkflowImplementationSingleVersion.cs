using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Execution;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Support;

namespace Nexus.Link.WorkflowEngine.Sdk;

/// <summary>
/// A simpler for of workflow implementation without any version handling
/// </summary>
public abstract class WorkflowImplementationSingleVersion : WorkflowImplementation
{
    private static readonly Dictionary<string, IWorkflowContainer> WorkflowContainers = new();

    private WorkflowContainerSingleVersion WorkflowContainerSingleVersion => (WorkflowContainerSingleVersion) WorkflowContainer;

    internal static IWorkflowContainer GetOrCreateWorkflowContainer(string capabilityName, string workflowTitle, string workflowId, IWorkflowEngineRequiredCapabilities workflowCapabilities)
    {
        lock (WorkflowContainers)
        {
            if (WorkflowContainers.TryGetValue(workflowId, out var workflowContainer)) return workflowContainer;
            workflowContainer = new WorkflowContainerSingleVersion(capabilityName, workflowTitle, workflowId, workflowCapabilities);
            WorkflowContainers.Add(workflowId, workflowContainer);
            return workflowContainer;
        }
    }

    /// <inheritdoc />
    protected WorkflowImplementationSingleVersion(string capabilityName, string workflowTitle, string workflowId, IWorkflowEngineRequiredCapabilities workflowCapabilities)
        : base(1, 0, 
            GetOrCreateWorkflowContainer(capabilityName, workflowTitle, workflowId, workflowCapabilities))
    {
        if (WorkflowContainerSingleVersion.WorkflowVersionCollection.Versions.Any()) return;
        WorkflowContainerSingleVersion.AddImplementation(this);
    }

    /// <inheritdoc />
    public override async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var implementation = await WorkflowContainerSingleVersion.SelectImplementationAsync(1, 1, cancellationToken);
        var implementationSingleVersion = (WorkflowImplementationSingleVersion) implementation;
        implementationSingleVersion.CopyParametersFrom(this);
        await implementationSingleVersion.ExecuteSingleVersionAsync(cancellationToken);
    }

    private void CopyParametersFrom(IWorkflowImplementation implementation)
    {
        var workflowImplementation = implementation as WorkflowImplementation;
        if (workflowImplementation == null) return;
        foreach (var (name, argument) in workflowImplementation.Arguments)
        {
            SetParameter(name, argument.Value);
        }
        workflowImplementation.Arguments.Clear();
    }

    internal async Task ExecuteSingleVersionAsync(CancellationToken cancellationToken)
    {
        await base.ExecuteAsync(cancellationToken);
    }
}

/// <summary>
/// A simpler for of workflow implementation without any version handling
/// </summary>
/// <typeparam name="TWorkflowResult">
/// The type for the result value for this workflow.
/// </typeparam>
public abstract class WorkflowImplementationSingleVersion<TWorkflowResult> : WorkflowImplementation<TWorkflowResult>
{
    private WorkflowContainerSingleVersion WorkflowContainerSingleVersion => (WorkflowContainerSingleVersion)WorkflowContainer;

    /// <inheritdoc />
    protected WorkflowImplementationSingleVersion(string capabilityName, string workflowTitle, string workflowId, IWorkflowEngineRequiredCapabilities workflowCapabilities)
        : base(1, 0, 
            WorkflowImplementationSingleVersion.GetOrCreateWorkflowContainer(capabilityName, workflowTitle, workflowId, workflowCapabilities))
    {
        if (WorkflowContainerSingleVersion.WorkflowVersionCollection.Versions.Any()) return;
        WorkflowContainerSingleVersion.AddImplementation(this);
    }

    /// <inheritdoc />
    public override async Task<TWorkflowResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var implementation = await WorkflowContainerSingleVersion.SelectImplementationAsync<TWorkflowResult>(1, 1, cancellationToken);
        var implementationSingleVersion = (WorkflowImplementationSingleVersion<TWorkflowResult>)implementation;
        var result = await implementationSingleVersion.ExecuteSingleVersionAsync(cancellationToken);
        return result;
    }

    internal async Task<TWorkflowResult> ExecuteSingleVersionAsync(CancellationToken cancellationToken)
    {
        var result = await base.ExecuteAsync(cancellationToken);
        return result;
    }
}