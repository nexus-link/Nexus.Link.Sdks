using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowState.Abstract;
using Nexus.Link.Components.WorkflowMgmt.Abstract;
using Nexus.Link.Components.WorkflowMgmt.Abstract.Services;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.Services.Administration;

namespace Nexus.Link.WorkflowEngine.Sdk.Services
{
    // TODO: Make this class internal when Privatmegleren no longer uses it
    /// <inheritdoc />
    public class WorkflowMgmtCapability : IWorkflowMgmtCapability
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public WorkflowMgmtCapability(IRuntimeTables runtimeTables, IWorkflowStateCapability stateCapability, IAsyncRequestMgmtCapability requestMgmtCapability)
        {
            Workflow = new WorkflowService(stateCapability, requestMgmtCapability);
            Activity = new ActivityService(runtimeTables, stateCapability, requestMgmtCapability);
        }

        /// <inheritdoc />
        public IActivityService Activity { get; }

        /// <inheritdoc />
        public IWorkflowService Workflow { get; }
    }
}