using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowState.Abstract;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Support;

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

        private Dictionary<string, ActivityDefinition> _activityDefinitions = new();

        protected WorkflowVersions(string capabilityName, string workflowTitle, string workflowId, IWorkflowEngineRequiredCapabilities workflowCapabilities)
        {
            WorkflowCapabilityName = capabilityName;
            WorkflowFormTitle = workflowTitle;
            WorkflowFormId = workflowId.ToGuidString();
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

        public void DefineActivity(string activityFormId, string title, ActivityTypeEnum type)
        {
            InternalContract.RequireNotNullOrWhiteSpace(activityFormId, nameof(activityFormId));
            InternalContract.RequireNotNullOrWhiteSpace(title, nameof(title));
            var id = activityFormId.ToGuidString();
            _activityDefinitions.Add(id, new ActivityDefinition
            {
                ActivityFormId = id,
                Title = title,
                Type = type
            });
        }

        public ActivityDefinition GetActivityDefinition(string activityFormId)
        {
            var id = activityFormId.ToGuidString();
            if (!_activityDefinitions.ContainsKey(id)) return null;
            return _activityDefinitions[id];
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