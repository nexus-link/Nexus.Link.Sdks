using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract;
using Nexus.Link.Capabilities.WorkflowState.Abstract;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;

namespace Nexus.Link.WorkflowEngine.Sdk.Support
{
    public class WorkflowInformation : IValidatable
    {
        public IWorkflowImplementationBase WorkflowImplementation { get; set; }

        public IWorkflowVersions WorkflowVersions => WorkflowImplementation.WorkflowVersions;

        public IAsyncRequestMgmtCapability AsyncRequestMgmtCapability => WorkflowVersions.AsyncRequestMgmtCapability;

        public IWorkflowConfigurationCapability ConfigurationCapability => WorkflowVersions.ConfigurationCapability;

        public IWorkflowStateCapability StateCapability => WorkflowVersions.StateCapability;
        public string CapabilityName => WorkflowVersions.WorkflowCapabilityName;
        public string FormTitle => WorkflowVersions.WorkflowFormTitle;
        public string InstanceTitle => WorkflowImplementation.GetInstanceTitle();
        public int MajorVersion => WorkflowImplementation.MajorVersion;
        public int MinorVersion => WorkflowImplementation.MinorVersion;
        public string FormId { get; }
        public string InstanceId { get; set; }
        public ActivityOptions DefaultActivityOptions { get; } = new();

        public WorkflowInformation(IWorkflowImplementationBase workflowImplementation)
        {
            WorkflowImplementation = workflowImplementation;
            FormId = WorkflowVersions.WorkflowFormId.ToLowerInvariant();
        }

        /// <inheritdoc />
        public void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsNotNull(ConfigurationCapability, nameof(ConfigurationCapability), errorLocation);
            FulcrumValidate.IsNotNull(StateCapability, nameof(StateCapability), errorLocation);
            FulcrumValidate.IsNotNullOrWhiteSpace(CapabilityName, nameof(CapabilityName), errorLocation);
            FulcrumValidate.IsNotNullOrWhiteSpace(FormId, nameof(FormId), errorLocation);
            FulcrumValidate.IsNotNullOrWhiteSpace(FormTitle, nameof(FormTitle), errorLocation);
            FulcrumValidate.IsGreaterThanOrEqualTo(0, MajorVersion, nameof(MajorVersion), errorLocation);
            FulcrumValidate.IsGreaterThanOrEqualTo(0, MinorVersion, nameof(MinorVersion), errorLocation);
            FulcrumValidate.IsNotNullOrWhiteSpace(InstanceId, nameof(InstanceId), errorLocation);
            FulcrumValidate.IsNotNullOrWhiteSpace(InstanceTitle, nameof(InstanceTitle), errorLocation);
        }
    }
}