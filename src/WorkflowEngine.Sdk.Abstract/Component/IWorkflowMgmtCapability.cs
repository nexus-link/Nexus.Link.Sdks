using Nexus.Link.WorkflowEngine.Sdk.Abstract.Component.Services;

namespace Nexus.Link.WorkflowEngine.Sdk.Abstract.Component;

public interface IWorkflowMgmtCapability
{
    IActivityService Activity { get; }
    IWorkflowService Workflow { get; }
    IInstanceService Instance { get; }
    IFormService Form{ get; }
    IFormOverviewService FormOverview { get; }
    IVersionService Version { get; }
}