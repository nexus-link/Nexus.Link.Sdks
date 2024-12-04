using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Services;


/// <summary>
/// Persistence for <see cref="WorkflowForm"/>
/// </summary>
public interface IWorkflowFormService : 
    ICreateWithSpecifiedId<WorkflowFormCreate, WorkflowForm, string>, 
    ICreateWithSpecifiedIdAndReturn<WorkflowFormCreate, WorkflowForm, string>, 
    IRead<WorkflowForm, string>, 
    IUpdate<WorkflowForm, string>, 
    IUpdateAndReturn<WorkflowForm, string>, 
    IReadAllWithPaging<WorkflowForm, string>
{
}