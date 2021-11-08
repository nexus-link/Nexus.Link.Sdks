using Nexus.Link.AsyncManager.Sdk;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;

namespace WorkflowEngine.Sdk.UnitTests.WorkflowLogic.Support
{
    public class TestWorkflowImplementation : IWorkflowImplementationBase
    {
        protected readonly IWorkflowMgmtCapability WorkflowCapability;
        protected readonly IAsyncRequestMgmtCapability AsyncRequestMgmtCapability;

        public TestWorkflowImplementation(IWorkflowMgmtCapability workflowCapability, IAsyncRequestMgmtCapability asyncRequestMgmtCapability)
        {
            WorkflowCapability = workflowCapability;
            AsyncRequestMgmtCapability = asyncRequestMgmtCapability;
            WorkflowVersions = new TestWorkflowVersions(workflowCapability, asyncRequestMgmtCapability);
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