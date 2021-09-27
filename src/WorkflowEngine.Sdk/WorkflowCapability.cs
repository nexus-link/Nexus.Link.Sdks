using AsyncManager.Sdk.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services;
using WorkflowEngine.Persistence.Abstract;
using WorkflowEngine.Sdk.Services;

namespace WorkflowEngine.Sdk
{
    public class WorkflowCapability : IWorkflowCapabilityForServer
    {
        public WorkflowCapability(IAsyncManagementCapabilityForClient asyncManagementCapability, IConfigurationTables configurationTables, IRuntimeTables runtimeTables)
        {
            AsyncContext = new AsyncContextService(asyncManagementCapability);
            WorkflowForm = new WorkflowFormService(configurationTables);
            WorkflowVersion= new WorkflowVersionService(configurationTables);
            WorkflowParameter= new WorkflowParameterService(configurationTables);
            ActivityForm = new ActivityFormService(configurationTables); 
            ActivityVersion = new ActivityVersionService(configurationTables);
            Transition = new TransitionService(configurationTables); 
            ActivityParameter= new ActivityParameterService(configurationTables);
            WorkflowInstance = new WorkflowInstanceService(runtimeTables);
            ActivityInstance = new ActivityInstanceService(runtimeTables); 
        }

        /// <inheritdoc />
        public IAsyncContextService AsyncContext { get; }

        /// <inheritdoc />
        public IWorkflowFormService WorkflowForm { get; }

        /// <inheritdoc />
        public IWorkflowVersionService WorkflowVersion { get; }

        /// <inheritdoc />
        public IWorkflowParameterService WorkflowParameter { get; }

        /// <inheritdoc />
        public IActivityFormService ActivityForm{ get; }

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
    }
}