using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Support;
using Nexus.Link.WorkflowEngine.Sdk.Support;

namespace WorkflowEngine.Sdk.UnitTests.TestSupport
{
    public class TestWorkflowContainer : IWorkflowContainer
    {
        public TestWorkflowContainer(IWorkflowEngineRequiredCapabilities workflowCapabilities)
        {
            WorkflowCapabilities = workflowCapabilities;
            _workflowVersionCollection = new WorkflowVersionCollection(this);
        }

        /// <inheritdoc />
        public IWorkflowEngineRequiredCapabilities WorkflowCapabilities { get; }

        /// <inheritdoc />
        public string WorkflowCapabilityName => "Workflow capability name";

        /// <inheritdoc />
        public string WorkflowFormId => "995974ED-829B-46CC-B1B3-2121A895F5F1".ToGuidString();

        /// <inheritdoc />
        public string WorkflowFormTitle => "Workflow form title";
        
        private readonly WorkflowVersionCollection _workflowVersionCollection;

        public ActivityDefinition GetActivityDefinition(string activityFormId) => new ActivityDefinition
        {
            ActivityFormId = activityFormId, Title = "Title", Type = ActivityTypeEnum.Action
        };

        /// <inheritdoc />
        public void AddImplementation(IWorkflowImplementationBase workflowImplementation)
        {
            throw new System.NotImplementedException();
        }
    }
}