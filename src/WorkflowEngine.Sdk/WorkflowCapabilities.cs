using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract;
using Nexus.Link.Capabilities.WorkflowState.Abstract;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;

namespace Nexus.Link.WorkflowEngine.Sdk;

public class WorkflowCapabilities : IWorkflowEngineRequiredCapabilities, IValidatable
{
    public IWorkflowConfigurationCapability ConfigurationCapability { get; set; }
    public IWorkflowStateCapability StateCapability { get; set; }
    public IAsyncRequestMgmtCapability RequestMgmtCapability { get; set; }

    public WorkflowCapabilities(IWorkflowConfigurationCapability configurationCapability, IWorkflowStateCapability stateCapability, IAsyncRequestMgmtCapability requestMgmtCapability)
    {
        ConfigurationCapability = configurationCapability;
        StateCapability = stateCapability;
        RequestMgmtCapability = requestMgmtCapability;
    }

    public void Validate(string errorLocation, string propertyPath = "")
    {
        FulcrumValidate.IsNotNull(ConfigurationCapability, nameof(ConfigurationCapability), errorLocation);
        FulcrumValidate.IsNotNull(StateCapability, nameof(StateCapability), errorLocation);
        FulcrumValidate.IsNotNull(RequestMgmtCapability, nameof(RequestMgmtCapability), errorLocation);
    }
}