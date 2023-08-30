using System.Threading;
using System.Threading.Tasks;

namespace Nexus.Link.WorkflowEngine.Sdk.Abstract.Execution;

/// <summary>
/// Functionality for things to do before and after the execution of a workflow
/// </summary>
public interface IWorkflowBeforeAndAfterExecution
{
    /// <summary>
    /// Prepare before executing the workflow. The most important part here is to load the latest state.
    /// </summary>
    Task BeforeExecutionAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Book keeping after executing the workflow. The most important part here is to save the state.
    /// </summary>
    Task AfterExecutionAsync(CancellationToken cancellationToken);
}