using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.AsyncManager.Sdk;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.WorkflowEngine.Sdk;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Support;

namespace WorkflowEngine.Sdk.UnitTests.WorkflowLogic.Support
{
    public class TestWorkflowImplementation : WorkflowImplementation
    {
        protected readonly IWorkflowMgmtCapability WorkflowCapability;
        protected readonly IAsyncRequestMgmtCapability AsyncRequestMgmtCapability;

        public TestWorkflowImplementation(IWorkflowMgmtCapability workflowCapability, IAsyncRequestMgmtCapability asyncRequestMgmtCapability)
        :base(1, 2, new TestWorkflowVersions(workflowCapability, asyncRequestMgmtCapability))
        {
            WorkflowCapability = workflowCapability;
            AsyncRequestMgmtCapability = asyncRequestMgmtCapability;
        }

        /// <inheritdoc />
        public override string GetInstanceTitle() => "Workflow instance title";

        /// <inheritdoc />
        public override IWorkflowImplementation CreateWorkflowInstance()
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public override Task ExecuteWorkflowAsync(CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}