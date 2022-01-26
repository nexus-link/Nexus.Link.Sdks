using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Services;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.Services.Configuration;

namespace Nexus.Link.WorkflowEngine.Sdk
{
    public class WorkflowConfigurationCapability : IWorkflowConfigurationCapability
    {
        public WorkflowConfigurationCapability(IConfigurationTables configurationTables)
        {
            WorkflowForm = new WorkflowFormService(configurationTables);
            WorkflowVersion = new WorkflowVersionService(configurationTables);
            ActivityForm = new ActivityFormService(configurationTables);
            ActivityVersion = new ActivityVersionService(configurationTables);
        }

        /// <inheritdoc />
        public IWorkflowFormService WorkflowForm { get; }

        /// <inheritdoc />
        public IWorkflowVersionService WorkflowVersion { get; }

        /// <inheritdoc />
        public IActivityFormService ActivityForm { get; }

        /// <inheritdoc />
        public IActivityVersionService ActivityVersion { get; }
    }
}