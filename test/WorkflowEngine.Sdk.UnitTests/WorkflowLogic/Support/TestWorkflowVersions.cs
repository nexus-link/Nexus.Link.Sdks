using Nexus.Link.AsyncManager.Sdk;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.WorkflowEngine.Sdk;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Logic;

namespace WorkflowEngine.Sdk.UnitTests.WorkflowLogic.Support
{
    public class TestWorkflowVersions : IWorkflowVersions
    {
        public TestWorkflowVersions(IWorkflowMgmtCapability workflowCapability, IAsyncRequestClient asyncRequestClient)
        {
            AsyncRequestClient = asyncRequestClient;
            WorkflowCapability = workflowCapability;
            WorkflowVersionCollection = new WorkflowVersionCollection(this);
        }

        /// <inheritdoc />
        public IAsyncRequestClient AsyncRequestClient { get; }

        /// <inheritdoc />
        public IWorkflowMgmtCapability WorkflowCapability { get; }

        /// <inheritdoc />
        public string WorkflowCapabilityName => "Workflow capability name";

        /// <inheritdoc />
        public string WorkflowFormId => "995974ED-829B-46CC-B1B3-2121A895F5F1";

        /// <inheritdoc />
        public string WorkflowFormTitle => "Workflow form title";

        /// <inheritdoc />
        public WorkflowVersionCollection WorkflowVersionCollection { get; }
    }
}