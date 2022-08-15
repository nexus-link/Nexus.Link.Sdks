using Nexus.Link.Components.WorkflowMgmt.Abstract;
using Nexus.Link.Components.WorkflowMgmt.Abstract.Services;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.Services.Administration;
using Nexus.Link.WorkflowEngine.Sdk.Services.State;

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
            Workflow = new WorkflowService(workflowEngineRequiredCapabilities.StateCapability, workflowEngineRequiredCapabilities.RequestMgmtCapability, runtimeTables);
            Activity = new ActivityService(workflowEngineRequiredCapabilities.StateCapability, workflowEngineRequiredCapabilities.RequestMgmtCapability);
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
        public IFormOverviewService FormOverview { get; }

        /// <inheritdoc />
        public IVersionService Version { get; }
    }
}