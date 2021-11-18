using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.Administration;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.Configuration;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.State;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.Services.Administration;
using Nexus.Link.WorkflowEngine.Sdk.Services.Configuration;
using Nexus.Link.WorkflowEngine.Sdk.Services.State;

namespace Nexus.Link.WorkflowEngine.Sdk
{
    public class WorkflowMgmtCapability : IWorkflowMgmtCapability
    {
        public WorkflowMgmtCapability(IConfigurationTables configurationTables, IRuntimeTables runtimeTables, IAsyncRequestMgmtCapability requestMgmtCapability)
        {
            WorkflowForm = new WorkflowFormService(configurationTables);
            WorkflowVersion = new WorkflowVersionService(configurationTables);
            WorkflowParameter = new WorkflowParameterService(configurationTables);
            ActivityForm = new ActivityFormService(configurationTables);
            ActivityVersion = new ActivityVersionService(configurationTables);
            Transition = new TransitionService(configurationTables);
            ActivityParameter = new ActivityParameterService(configurationTables);
            WorkflowInstance = new WorkflowInstanceService(runtimeTables);
            ActivityInstance = new ActivityInstanceService(runtimeTables, requestMgmtCapability);
            Log = new LogService(runtimeTables);
            WorkflowSummary = new WorkflowSummaryService(configurationTables, runtimeTables, requestMgmtCapability);
            Workflow = new WorkflowService(this, requestMgmtCapability);
        }

        /// <inheritdoc />
        public IWorkflowFormService WorkflowForm { get; }

        /// <inheritdoc />
        public IWorkflowVersionService WorkflowVersion { get; }

        /// <inheritdoc />
        public IWorkflowParameterService WorkflowParameter { get; }

        /// <inheritdoc />
        public IActivityFormService ActivityForm { get; }

        /// <inheritdoc />
        public IActivityVersionService ActivityVersion { get; }

        /// <inheritdoc />
        public IActivityInstanceService ActivityInstance { get; }

        /// <inheritdoc />
        public ILogService Log { get; }

        /// <inheritdoc />
        public ITransitionService Transition { get; }

        /// <inheritdoc />
        public IActivityParameterService ActivityParameter { get; }

        /// <inheritdoc />
        public IWorkflowInstanceService WorkflowInstance { get; }

        /// <inheritdoc />
        public IWorkflowSummaryService WorkflowSummary { get; }

        /// <inheritdoc />
        public IWorkflowService Workflow { get; }
    }
}