using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Abstract.Component.Services;

/// <summary>
/// Management methods for a workflow form
/// </summary>
public interface IFormService : IRead<WorkflowForm, string>, IReadAllWithPaging<WorkflowForm, string>
{
}