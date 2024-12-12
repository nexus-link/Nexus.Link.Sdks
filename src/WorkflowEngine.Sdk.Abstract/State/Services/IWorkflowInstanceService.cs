using System.Threading.Tasks;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Services;

/// <summary>
/// Persistence for <see cref="WorkflowInstance"/>
/// </summary>
public interface IWorkflowInstanceService : 
    ICreateWithSpecifiedId<WorkflowInstanceCreate,WorkflowInstance, string>, 
    ICreateWithSpecifiedIdAndReturn<WorkflowInstanceCreate,WorkflowInstance, string>, 
    IRead<WorkflowInstance, string>, 
    IUpdate<WorkflowInstance, string>,
    IUpdateAndReturn<WorkflowInstance, string>,
    IDistributedLock<string>
{
    /// <summary>
    /// Default global workflow options
    /// </summary>
    WorkflowOptions DefaultWorkflowOptions { get; }
}

/// <summary>
/// Delegate for when a <see cref="WorkflowInstance"/> has been saved.
/// </summary>
public delegate Task AfterSaveDelegate(
    WorkflowForm oldForm, WorkflowVersion oldVersion, WorkflowInstance oldInstance,
    WorkflowForm newForm, WorkflowVersion newVersion, WorkflowInstance newInstance);