using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract.Services;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.Services;

namespace Nexus.Link.WorkflowEngine.Sdk
{
    public class WorkflowCapability : IWorkflowCapability
    {
        public WorkflowCapability(IConfigurationTables configurationTables, IRuntimeTables runtimeTables, IAsyncRequestMgmtCapability requestMgmtCapability)
        {
            WorkflowForm = new WorkflowFormService(configurationTables);
            WorkflowVersion = new WorkflowVersionService(configurationTables);
            WorkflowParameter = new WorkflowParameterService(configurationTables);
            ActivityForm = new ActivityFormService(configurationTables);
            ActivityVersion = new ActivityVersionService(configurationTables);
            Transition = new TransitionService(configurationTables);
            ActivityParameter = new ActivityParameterService(configurationTables);
            WorkflowInstance = new WorkflowInstanceService(runtimeTables);
            ActivityInstance = new ActivityInstanceService(runtimeTables);
            Workflow = new WorkflowService(configurationTables, runtimeTables);
            WorkflowAdministrationService = new WorkflowAdministrationService(Workflow, WorkflowInstance, requestMgmtCapability);
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
        public ITransitionService Transition { get; }

        /// <inheritdoc />
        public IActivityParameterService ActivityParameter { get; }

        /// <inheritdoc />
        public IWorkflowInstanceService WorkflowInstance { get; }

        /// <inheritdoc />
        public IWorkflowService Workflow { get; }

        /// <inheritdoc />
        public IWorkflowAdministrationService WorkflowAdministrationService { get; }
    }
}