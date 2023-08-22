using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Services;

namespace Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration;

public interface IWorkflowConfigurationCapability
{
    IWorkflowFormService WorkflowForm { get; }
    IWorkflowVersionService WorkflowVersion { get; }
    IActivityFormService ActivityForm { get; }
    IActivityVersionService ActivityVersion { get; }
}