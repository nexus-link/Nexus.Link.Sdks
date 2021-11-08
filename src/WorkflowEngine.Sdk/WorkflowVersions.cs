using Nexus.Link.AsyncManager.Sdk;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Logic;

namespace Nexus.Link.WorkflowEngine.Sdk
{
    public abstract class WorkflowVersions : IWorkflowVersions
    {

        /// <inheritdoc />
        public IAsyncRequestMgmtCapability AsyncRequestMgmtCapability { get; protected set; }

        /// <inheritdoc />
        public IWorkflowMgmtCapability WorkflowCapability { get; protected set;}

        /// <inheritdoc />
        public string WorkflowCapabilityName { get; }

        /// <inheritdoc />
        public string WorkflowFormId { get; }

        /// <inheritdoc />
        public string WorkflowFormTitle { get; }

        /// <inheritdoc />
        public WorkflowVersionCollection WorkflowVersionCollection { get; }
        
        protected WorkflowVersions(string capabilityName, string workflowTitle, string workflowId)
        {
            WorkflowCapabilityName = capabilityName;
            WorkflowFormTitle = workflowTitle;
            WorkflowFormId = workflowId;
            WorkflowVersionCollection = new WorkflowVersionCollection(this);
        }

        protected void AddWorkflowVersion(IWorkflowImplementationBase workflowImplementation)
        {
            WorkflowVersionCollection.AddWorkflowVersion(workflowImplementation);
        }

        public IWorkflowImplementation<TWorkflowResult> SelectWorkflowVersion<TWorkflowResult>(int minVersion, int maxVersion)
        {
            return WorkflowVersionCollection.SelectWorkflowVersion<TWorkflowResult>(minVersion, maxVersion);
        }

        public IWorkflowImplementation SelectWorkflowVersion(int minVersion, int maxVersion)
        {
            return WorkflowVersionCollection.SelectWorkflowVersion(minVersion, maxVersion);
        }
    }
}