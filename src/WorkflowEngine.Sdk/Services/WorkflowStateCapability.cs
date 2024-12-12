using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Execution;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Services;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.Services.State;

namespace Nexus.Link.WorkflowEngine.Sdk.Services
{
    // TODO: Make this class internal when Privatmegleren no longer uses it
    /// <inheritdoc />
    public class WorkflowStateCapability : IWorkflowStateCapability
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public WorkflowStateCapability(IConfigurationTables configurationTables, IRuntimeTables runtimeTables,
            IWorkflowEngineStorage storage, IAsyncRequestMgmtCapability requestMgmtCapability, WorkflowOptions workflowOptions)
        {
            WorkflowInstance = new WorkflowInstanceService(runtimeTables, workflowOptions);
            ActivityInstance = new ActivityInstanceService(runtimeTables, requestMgmtCapability);
            Log = new LogService(runtimeTables);
            WorkflowSummary = new WorkflowSummaryService(configurationTables, runtimeTables);
            WorkflowSummaryStorage = new WorkflowSummaryServiceStorage(storage);
            WorkflowSemaphore = new WorkflowSemaphoreService(requestMgmtCapability, runtimeTables);
        }

        /// <inheritdoc />
        public IActivityInstanceService ActivityInstance { get; }

        /// <inheritdoc />
        public ILogService Log { get; }

        /// <inheritdoc />
        public IWorkflowInstanceService WorkflowInstance { get; }

        /// <inheritdoc />
        public IWorkflowSummaryService WorkflowSummary { get; }

        /// <inheritdoc />
        public IWorkflowSummaryServiceStorage WorkflowSummaryStorage { get; }

        /// <inheritdoc />
        public IWorkflowSemaphoreService WorkflowSemaphore { get; }
    }
}