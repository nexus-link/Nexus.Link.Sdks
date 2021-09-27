using System.Collections.Generic;
using System.Linq;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;

namespace WorkflowEngine.Sdk.WorkflowLogic
{
    public class WorkflowVersionCollection : IWorkflowVersionCollection
    {
        public IWorkflowCapabilityForClient Capability { get; }
        private readonly Dictionary<int, object> _versions = new Dictionary<int, object>();

        public string WorkflowCapabilityName { get; private set; }
        public string WorkflowFormId { get; set; }
        public string WorkflowFormTitle { get; set; }

        protected WorkflowVersionCollection(IWorkflowCapabilityForClient workflowCapability)
        {
            Capability = workflowCapability;
        }

        protected WorkflowVersionCollection DefineWorkflow(string capabilityName, string workflowTitle, string workflowFormId)
        {
            InternalContract.RequireNotNullOrWhiteSpace(capabilityName, nameof(capabilityName));
            InternalContract.RequireNotNullOrWhiteSpace(workflowTitle, nameof(workflowTitle));
            InternalContract.RequireNotNullOrWhiteSpace(workflowFormId, nameof(workflowFormId));
            WorkflowCapabilityName = capabilityName;
            WorkflowFormId = workflowFormId;
            WorkflowFormTitle = workflowTitle;
            return this;
        }

        /// <inheritdoc />
        public WorkflowVersion<TResponse> SelectWorkflowVersion<TResponse>(int minVersion, int maxVersion)
        {
            foreach (var version in _versions.Where(pair => pair.Key >= minVersion && pair.Key <= maxVersion))
            {
                var implementation = version.Value as WorkflowVersion<TResponse>;
                FulcrumAssert.IsNotNull(implementation, CodeLocation.AsString());
                var instance = implementation!.CreateWorkflowInstance();
                return instance;
            }

            return null;
        }

        public WorkflowVersionCollection AddWorkflowVersion<TResponse>(WorkflowVersion<TResponse> workflowVersion)
        {
            _versions.Add(workflowVersion.MajorVersion, workflowVersion);
            return this;
        }
    }
}