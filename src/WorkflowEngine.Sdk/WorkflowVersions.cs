using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract;
using Nexus.Link.Capabilities.WorkflowState.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Logic;

namespace Nexus.Link.WorkflowEngine.Sdk
{
    public abstract class WorkflowVersions : IWorkflowVersions
    {

        /// <inheritdoc />
        public IWorkflowEngineRequiredCapabilities WorkflowCapabilities { get; }

        /// <inheritdoc />
        public string WorkflowCapabilityName { get; }

        /// <inheritdoc />
        public string WorkflowFormId { get; }

        /// <inheritdoc />
        public string WorkflowFormTitle { get; }

        /// <inheritdoc />
        public WorkflowVersionCollection WorkflowVersionCollection { get; }
        
        protected WorkflowVersions(string capabilityName, string workflowTitle, string workflowId, IWorkflowEngineRequiredCapabilities workflowCapabilities)
        {
            WorkflowCapabilityName = capabilityName;
            WorkflowFormTitle = workflowTitle;
            WorkflowFormId = workflowId.ToLowerInvariant();
            WorkflowVersionCollection = new WorkflowVersionCollection(this);
            WorkflowCapabilities = workflowCapabilities;
        }

        protected void AddWorkflowVersion(IWorkflowImplementationBase workflowImplementation)
        {
            WorkflowVersionCollection.AddWorkflowVersion(workflowImplementation);
        }

        public Task<IWorkflowImplementation<TWorkflowResult>> SelectWorkflowVersionAsync<TWorkflowResult>(int minVersion, int maxVersion, CancellationToken cancellationToken = default)
        {
            return WorkflowVersionCollection.SelectWorkflowVersionAsync<TWorkflowResult>(minVersion, maxVersion, cancellationToken);
        }

        public Task<IWorkflowImplementation<TWorkflowResult>> SelectWorkflowVersionAsync<TWorkflowResult>(int minVersion, CancellationToken cancellationToken = default)
        {
            return WorkflowVersionCollection.SelectWorkflowVersionAsync<TWorkflowResult>(minVersion, null, cancellationToken);
        }

        public Task<IWorkflowImplementation> SelectWorkflowVersionAsync(int minVersion, int maxVersion, CancellationToken cancellationToken = default)
        {
            return WorkflowVersionCollection.SelectWorkflowVersionAsync(minVersion, maxVersion, cancellationToken);
        }

        public Task<IWorkflowImplementation> SelectWorkflowVersionAsync(int minVersion, CancellationToken cancellationToken = default)
        {
            return WorkflowVersionCollection.SelectWorkflowVersionAsync(minVersion, null, cancellationToken);
        }

        [Obsolete("Use SelectWorkflowVersionAsync. Compilation warning since 2021-11-12. Will be Compilation error after 2021-12-01")]
        public IWorkflowImplementation<TWorkflowResult> SelectWorkflowVersion<TWorkflowResult>(int minVersion, int maxVersion)
        {
            return WorkflowVersionCollection.SelectWorkflowVersionAsync<TWorkflowResult>(minVersion, maxVersion).Result;
        }

        [Obsolete("Use SelectWorkflowVersionAsync. Compilation warning since 2021-11-12. Will be Compilation error after 2021-12-01")]
        public IWorkflowImplementation SelectWorkflowVersion(int minVersion, int maxVersion)
        {
            return WorkflowVersionCollection.SelectWorkflowVersionAsync(minVersion, maxVersion).Result;
        }

        /// <inheritdoc />
        public override string ToString() => $"{WorkflowCapabilityName}.{WorkflowFormTitle}";
    }
}