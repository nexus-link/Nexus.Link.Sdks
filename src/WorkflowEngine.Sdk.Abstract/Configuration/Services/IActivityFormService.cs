using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Services;

public interface IActivityFormService : ICreateWithSpecifiedIdAndReturn<ActivityFormCreate,ActivityForm, string>, IRead<ActivityForm, string>, IUpdateAndReturn<ActivityForm, string>
{
}