using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Activities;

namespace Nexus.Link.WorkflowEngine.Sdk.Abstract.Execution;

/// <summary>
/// The functionality needed for executing a workflow
/// </summary>
public interface IWorkflowExecutor : IWorkflowLogger
{
    /// <summary>
    /// Execute the <paramref name="workflowImplementation"/>
    /// </summary>
    Task ExecuteAsync(
        IWorkflowImplementation workflowImplementation,
        CancellationToken cancellationToken);

    /// <summary>
    /// Execute the <paramref name="workflowImplementation"/>
    /// </summary>
    Task<TWorkflowResult> ExecuteAsync<TWorkflowResult>(
        IWorkflowImplementation<TWorkflowResult> workflowImplementation,
        CancellationToken cancellationToken);

    /// <summary>
    /// Define a new parameter with name <paramref name="name"/> and type <typeparamref name="T"/>.
    /// </summary>
    void DefineParameter<T>(string name);

    /// <summary>
    /// Set the parameter <paramref name="name"/> to <paramref name="value"/>
    /// </summary>
    void SetParameter<TParameter>(string name, TParameter value);

    /// <summary>
    /// Get the value for the argument of parameter with name <paramref name="name"/>.
    /// </summary>
    T GetArgument<T>(string name);

    /// <summary>
    /// Create an activity with position <paramref name="position"/> and id <paramref name="id"/>
    /// </summary>
    IActivityFlow<TActivityReturns> CreateActivity<TActivityReturns>(int position, string id);

    /// <summary>
    /// Create an activity with position <paramref name="position"/> and id <paramref name="id"/>
    /// </summary>
    IActivityFlow CreateActivity(int position, string id);
}