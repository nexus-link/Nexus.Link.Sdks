using System.Threading.Tasks;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Services;

public interface IWorkflowInstanceService : 
    ICreateWithSpecifiedId<WorkflowInstanceCreate,WorkflowInstance, string>, 
    IRead<WorkflowInstance, string>, 
    IUpdate<WorkflowInstance, string>,
    IDistributedLock<string>
{
    /// <summary>
    /// Default global workflow options
    /// </summary>
    WorkflowOptions DefaultWorkflowOptions { get; }
}

/// <summary>
/// Global workflow options
/// </summary>
public class WorkflowOptions
{
    /// <summary>
    /// After a <see cref="WorkflowInstance"/> has been saved,
    /// this will be called with the old versions of the form, version and instance
    /// along with the new versions, enabling us to compare them and take action.
    /// </summary>
    public AfterSaveDelegate AfterSaveAsync { get; set; }

    /// <summary>
    /// Copy the options from <paramref name="source"/>.
    /// </summary>
    public WorkflowOptions From(WorkflowOptions source)
    {
        AfterSaveAsync = source.AfterSaveAsync;
        return this;
    }

}

/// <summary>
/// Delegate for when a <see cref="WorkflowInstance"/> has been saved.
/// </summary>
public delegate Task AfterSaveDelegate(
    WorkflowForm oldForm, WorkflowVersion oldVersion, WorkflowInstance oldInstance,
    WorkflowForm newForm, WorkflowVersion newVersion, WorkflowInstance newInstance);