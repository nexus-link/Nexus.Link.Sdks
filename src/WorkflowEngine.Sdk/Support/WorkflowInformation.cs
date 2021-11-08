using Nexus.Link.AsyncManager.Sdk;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.State;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;

namespace Nexus.Link.WorkflowEngine.Sdk.Support
{
    public class WorkflowInformation : IValidatable
    {
        public IWorkflowImplementationBase WorkflowImplementation { get; set; }

        public IWorkflowVersions WorkflowVersions =>
            WorkflowImplementation.WorkflowVersions;

        public IAsyncRequestMgmtCapability AsyncRequestMgmtCapability => WorkflowVersions.AsyncRequestMgmtCapability;

        public IWorkflowMgmtCapability WorkflowCapability => WorkflowVersions.WorkflowCapability;
        public string CapabilityName => WorkflowVersions.WorkflowCapabilityName;
        public string FormId => WorkflowVersions.WorkflowFormId;
        public string FormTitle => WorkflowVersions.WorkflowFormTitle;
        public string InstanceTitle => WorkflowImplementation.GetInstanceTitle();
        public int MajorVersion => WorkflowImplementation.MajorVersion;
        public int MinorVersion => WorkflowImplementation.MinorVersion;
        public string InstanceId { get; set; }
        public ActivityFailUrgencyEnum DefaultFailUrgency { get; set; } = ActivityFailUrgencyEnum.Stopping;
        public ActivityExceptionAlertHandler DefaultExceptionAlertHandler { get; set; }
        public double DefaultAsyncRequestPriority { get; set; } = 0.5;

        public WorkflowInformation(IWorkflowImplementationBase workflowImplementation)
        {
            WorkflowImplementation = workflowImplementation;
        }

        /// <inheritdoc />
        public void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsNotNull(WorkflowCapability, nameof(WorkflowCapability), errorLocation);
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