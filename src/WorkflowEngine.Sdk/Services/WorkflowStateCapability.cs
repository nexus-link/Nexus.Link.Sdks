using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowState.Abstract;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Messages;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Services;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Queue.Model;
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
        public WorkflowStateCapability(IConfigurationTables configurationTables, IRuntimeTables runtimeTables, IAsyncRequestMgmtCapability requestMgmtCapability)
        {
            WorkflowInstance = new WorkflowInstanceService(runtimeTables);
            ActivityInstance = new ActivityInstanceService(runtimeTables, requestMgmtCapability);
            Log = new LogService(runtimeTables);
            WorkflowSummary = new WorkflowSummaryService(configurationTables, runtimeTables, requestMgmtCapability);
            WorkflowSemaphore = new WorkflowSemaphoreService(requestMgmtCapability, runtimeTables);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public WorkflowStateCapability(IConfigurationTables configurationTables, IRuntimeTables runtimeTables, IAsyncRequestMgmtCapability requestMgmtCapability, IWritableQueue<WorkflowInstanceChangedV1> messageQueue)
        : this(configurationTables, runtimeTables, requestMgmtCapability)
        {
            if (messageQueue != null)
            {
                var sourceClientId = FulcrumApplication.Setup.ClientName ?? FulcrumApplication.Setup.Name;
                WorkflowMessageService = new WorkflowMessageService(sourceClientId, messageQueue, Log); // TODO: Log in this way?
            }
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
        public IWorkflowSemaphoreService WorkflowSemaphore { get; }

        /// <inheritdoc />
        public IWorkflowMessageService WorkflowMessageService { get; }
    }
}