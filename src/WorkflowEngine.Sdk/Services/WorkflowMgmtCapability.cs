using Nexus.Link.WorkflowEngine.Sdk.Abstract.Component;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Component.Services;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
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
        public WorkflowMgmtCapability(IWorkflowEngineRequiredCapabilities workflowEngineRequiredCapabilities, IRuntimeTables runtimeTables, IConfigurationTables configurationTables)
        {
            Workflow = new WorkflowService(workflowEngineRequiredCapabilities.StateCapability, workflowEngineRequiredCapabilities.ConfigurationCapability, workflowEngineRequiredCapabilities.RequestMgmtCapability, runtimeTables);
            Activity = new ActivityService(workflowEngineRequiredCapabilities.StateCapability, workflowEngineRequiredCapabilities.RequestMgmtCapability);
            Form = new FormService(workflowEngineRequiredCapabilities.ConfigurationCapability);
            FormOverview = new FormOverviewService(configurationTables);
            Version = new VersionService(workflowEngineRequiredCapabilities.ConfigurationCapability);
            Instance = new InstanceService(runtimeTables);
        }

        /// <inheritdoc />
        public IActivityService Activity { get; }

        /// <inheritdoc />
        public IWorkflowService Workflow { get; }

        /// <inheritdoc />
        public IInstanceService Instance { get; }

        /// <inheritdoc />
        public IFormService Form { get; }

        /// <inheritdoc />
        public IFormOverviewService FormOverview { get; }

        /// <inheritdoc />
        public IVersionService Version { get; }
    }
}