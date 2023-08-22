using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Services;

public interface IActivityVersionService : ICreateWithSpecifiedIdAndReturn<ActivityVersionCreate, ActivityVersion, string>, IRead<ActivityVersion, string>, IUpdateAndReturn<ActivityVersion, string>
{
}