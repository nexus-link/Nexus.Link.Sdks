using Nexus.Link.AsyncManager.Sdk;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;

namespace WorkflowEngine.Sdk.UnitTests.WorkflowLogic.Support
{
    public class TestWorkflowImplementation : IWorkflowImplementationBase
    {
        protected readonly IWorkflowMgmtCapability WorkflowCapability;
        protected readonly IAsyncRequestClient AsyncRequestClient;

        public TestWorkflowImplementation(IWorkflowMgmtCapability workflowCapability, IAsyncRequestClient asyncRequestClient)
        {
            WorkflowCapability = workflowCapability;
            AsyncRequestClient = asyncRequestClient;
            WorkflowVersions = new TestWorkflowVersions(workflowCapability, asyncRequestClient);
        }

        /// <inheritdoc />
        public int MajorVersion => 1;

        /// <inheritdoc />
        public int MinorVersion => 2;

        /// <inheritdoc />
        public string GetInstanceTitle() => "Workflow instance title";

        /// <inheritdoc />
        public IWorkflowVersions WorkflowVersions { get; }
    }
}