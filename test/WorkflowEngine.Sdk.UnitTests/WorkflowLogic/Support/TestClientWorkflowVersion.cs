using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract;
using Nexus.Link.Capabilities.WorkflowState.Abstract;
using Nexus.Link.WorkflowEngine.Sdk;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;

namespace WorkflowEngine.Sdk.UnitTests.WorkflowLogic.Support
{
    public class TestWorkflowImplementation : WorkflowImplementation
    {
        protected readonly IWorkflowConfigurationCapability ConfigurationCapability;
        protected readonly IWorkflowStateCapability StateCapability;
        protected readonly IAsyncRequestMgmtCapability AsyncRequestMgmtCapability;

        public TestWorkflowImplementation(IWorkflowConfigurationCapability configurationCapability, IWorkflowStateCapability stateCapability, IAsyncRequestMgmtCapability asyncRequestMgmtCapability)
        :base(1, 2, new TestWorkflowVersions(configurationCapability, stateCapability, asyncRequestMgmtCapability))
        {
            ConfigurationCapability = configurationCapability;
            StateCapability = stateCapability;
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