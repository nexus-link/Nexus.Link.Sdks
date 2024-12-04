using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Services;

/// <summary>
/// Persistence for <see cref="WorkflowVersion"/>
/// </summary>
public interface IWorkflowVersionService : 
    ICreateWithSpecifiedIdAndReturn<WorkflowVersionCreate, WorkflowVersion, string>, 
    IRead<WorkflowVersion, string>,
    IUpdateAndReturn<WorkflowVersion, string>
{
}