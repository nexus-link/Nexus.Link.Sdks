using System.Collections.Generic;
using System.Linq;
using Nexus.Link.AsyncManager.Sdk;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;

namespace Nexus.Link.WorkflowEngine.Sdk.Logic
{
    public class WorkflowVersionCollection
    {
        public IWorkflowVersions WorkflowVersions { get; }
        public IWorkflowMgmtCapability Capability => WorkflowVersions.WorkflowCapability;
        public IAsyncRequestMgmtCapability AsyncRequestMgmtCapability => WorkflowVersions.AsyncRequestMgmtCapability;

        private readonly Dictionary<int, IWorkflowImplementationBase> _versions = new();

        public WorkflowVersionCollection(IWorkflowVersions workflowVersions)
        {
            WorkflowVersions = workflowVersions;
        }
        
        public IWorkflowImplementation<TResponse> SelectWorkflowVersion<TResponse>(int minVersion, int maxVersion)
        {
            foreach (var version in _versions.Where(pair => pair.Key >= minVersion && pair.Key <= maxVersion))
            {
                var implementation = version.Value as IWorkflowImplementation<TResponse>;
                FulcrumAssert.IsNotNull(implementation, CodeLocation.AsString());
                var instance = implementation!.CreateWorkflowInstance();
                return instance;
            }

            return null;
        }

        public IWorkflowImplementation SelectWorkflowVersion(int minVersion, int maxVersion)
        {
            foreach (var version in _versions.Where(pair => pair.Key >= minVersion && pair.Key <= maxVersion))
            {
                var implementation = version.Value as IWorkflowImplementation;
                FulcrumAssert.IsNotNull(implementation, CodeLocation.AsString());
                var instance = implementation!.CreateWorkflowInstance();
                return instance;
            }

            return null;
        }

        public WorkflowVersionCollection AddWorkflowVersion(IWorkflowImplementationBase workflowImplementation)
        {
            _versions.Add(workflowImplementation.MajorVersion, workflowImplementation);
            return this;
        }
    }
}