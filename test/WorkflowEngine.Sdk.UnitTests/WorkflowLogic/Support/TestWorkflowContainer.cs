using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowState.Abstract;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Support;

namespace WorkflowEngine.Sdk.UnitTests.WorkflowLogic.Support
{
    public class TestWorkflowContainer : IWorkflowContainer
    {
        public TestWorkflowContainer(IWorkflowEngineRequiredCapabilities workflowCapabilities)
        {
            WorkflowCapabilities = workflowCapabilities;
            WorkflowVersionCollection = new WorkflowVersionCollection(this);
        }

        /// <inheritdoc />
        public IWorkflowEngineRequiredCapabilities WorkflowCapabilities { get; }

        /// <inheritdoc />
        public string WorkflowCapabilityName => "Workflow capability name";

        /// <inheritdoc />
        public string WorkflowFormId => "995974ED-829B-46CC-B1B3-2121A895F5F1".ToGuidString();

        /// <inheritdoc />
        public string WorkflowFormTitle => "Workflow form title";

        /// <inheritdoc />
        public WorkflowVersionCollection WorkflowVersionCollection { get; }

        public ActivityDefinition GetActivityDefinition(string activityFormId) => new ActivityDefinition
        {
            ActivityFormId = activityFormId, Title = "Title", Type = ActivityTypeEnum.Action
        };
    }
}