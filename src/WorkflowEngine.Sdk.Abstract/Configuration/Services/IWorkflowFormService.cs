using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Services;

public interface IWorkflowFormService : ICreateWithSpecifiedId<WorkflowFormCreate, WorkflowForm, string>, IRead<WorkflowForm, string>, IUpdate<WorkflowForm, string>, IReadAllWithPaging<WorkflowForm, string>
{
}