using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Support;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Model
{
    internal class WorkflowInformation : IValidatable
    {
        public IWorkflowImplementationBase WorkflowImplementation { get; set; }

        public IWorkflowContainer WorkflowContainer => WorkflowImplementation.WorkflowContainer;

        public IWorkflowEngineRequiredCapabilities WorkflowCapabilities => WorkflowContainer.WorkflowCapabilities;
        public string CapabilityName => WorkflowContainer.WorkflowCapabilityName;
        public string FormTitle => WorkflowContainer.WorkflowFormTitle;
        public string InstanceTitle => WorkflowImplementation.GetInstanceTitle();
        public int MajorVersion => WorkflowImplementation.MajorVersion;
        public int MinorVersion => WorkflowImplementation.MinorVersion;
        public string FormId { get; }
        public string InstanceId { get; set; }
        public ActivityOptions DefaultActivityOptions { get; } = new();

        public WorkflowInformation(IWorkflowImplementationBase workflowImplementation)
        {
            WorkflowImplementation = workflowImplementation;
            FormId = WorkflowContainer.WorkflowFormId.ToGuidString();
        }

        /// <inheritdoc />
        public void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsNotNull(WorkflowCapabilities, nameof(WorkflowCapabilities), errorLocation);
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