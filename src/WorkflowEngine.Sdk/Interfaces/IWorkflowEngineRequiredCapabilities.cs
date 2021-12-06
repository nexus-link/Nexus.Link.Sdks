using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract;
using Nexus.Link.Capabilities.WorkflowState.Abstract;

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces;

public interface IWorkflowEngineRequiredCapabilities
{
    IWorkflowConfigurationCapability ConfigurationCapability { get; }
    IWorkflowStateCapability StateCapability { get; }
    IAsyncRequestMgmtCapability RequestMgmtCapability{ get; }
}