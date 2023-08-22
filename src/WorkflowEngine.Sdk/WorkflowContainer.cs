using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Support;
using Nexus.Link.WorkflowEngine.Sdk.Support;

namespace Nexus.Link.WorkflowEngine.Sdk
{
    /// <inheritdoc />
    public abstract class WorkflowContainer : IWorkflowContainer
    {

        /// <inheritdoc />
        public IWorkflowEngineRequiredCapabilities WorkflowCapabilities { get; }

        /// <inheritdoc />
        public string WorkflowCapabilityName { get; }

        /// <inheritdoc />
        public string WorkflowFormId { get; }

        /// <inheritdoc />
        public string WorkflowFormTitle { get; }

        private readonly WorkflowVersionCollection _workflowVersionCollection;

        private readonly Dictionary<string, ActivityDefinition> _activityDefinitions = new();

        /// <summary>
        /// Constructor
        /// </summary>
        protected WorkflowContainer(string capabilityName, string workflowTitle, string workflowId, IWorkflowEngineRequiredCapabilities workflowCapabilities)
        {
            WorkflowCapabilityName = capabilityName;
            WorkflowFormTitle = workflowTitle;
            WorkflowFormId = workflowId.ToGuidString();
            _workflowVersionCollection = new WorkflowVersionCollection(this);
            WorkflowCapabilities = workflowCapabilities;
        }

        /// <summary>
        /// Add a <paramref name="workflowImplementation"/> to this container.
        /// </summary>
        [Obsolete("Please use AddImplementation. Obsolete since 2022-02-10")]
        protected void AddWorkflowVersion(IWorkflowImplementationBase workflowImplementation)
        {
            _workflowVersionCollection.AddWorkflowVersion(workflowImplementation);
        }

        /// <inheritdoc />
        public void AddImplementation(IWorkflowImplementationBase workflowImplementation)
        {
            _workflowVersionCollection.AddWorkflowVersion(workflowImplementation);
        }

        /// <summary>
        /// Select that latest <see cref="IWorkflowImplementation"/> with a <see cref="IWorkflowImplementationBase.MajorVersion"/>
        /// in the span <paramref name="minVersion"/> to <paramref name="maxVersion"/>.
        /// </summary>
        [Obsolete("Please use SelectImplementationAsync. Obsolete since 2022-02-10")]
        public Task<IWorkflowImplementation<TWorkflowResult>> SelectWorkflowVersionAsync<TWorkflowResult>(int minVersion, int maxVersion, CancellationToken cancellationToken = default)
        {
            return _workflowVersionCollection.SelectWorkflowVersionAsync<TWorkflowResult>(minVersion, maxVersion, cancellationToken);
        }

        /// <summary>
        /// Select that latest <see cref="IWorkflowImplementation"/> with a <see cref="IWorkflowImplementationBase.MajorVersion"/>
        /// in the span <paramref name="minVersion"/> to <paramref name="maxVersion"/>.
        /// </summary>
        public Task<IWorkflowImplementation<TWorkflowResult>> SelectImplementationAsync<TWorkflowResult>(int minVersion, int maxVersion, CancellationToken cancellationToken = default)
        {
            return _workflowVersionCollection.SelectWorkflowVersionAsync<TWorkflowResult>(minVersion, maxVersion, cancellationToken);
        }

        /// <summary>
        /// Select that latest <see cref="IWorkflowImplementation"/> with a <see cref="IWorkflowImplementationBase.MajorVersion"/>
        /// that is greater than or equal to <paramref name="minVersion"/>.
        /// </summary>
        [Obsolete("Please use SelectImplementationAsync. Obsolete since 2022-02-10")]
        public Task<IWorkflowImplementation<TWorkflowResult>> SelectWorkflowVersionAsync<TWorkflowResult>(int minVersion, CancellationToken cancellationToken = default)
        {
            return _workflowVersionCollection.SelectWorkflowVersionAsync<TWorkflowResult>(minVersion, null, cancellationToken);
        }

        /// <summary>
        /// Select that latest <see cref="IWorkflowImplementation"/> with a <see cref="IWorkflowImplementationBase.MajorVersion"/>
        /// that is greater than or equal to <paramref name="minVersion"/>.
        /// </summary>
        [Obsolete("Please use the overload with a maxVersion parameter. Obsolete since 2022-05-05.")]
        public Task<IWorkflowImplementation<TWorkflowResult>> SelectImplementationAsync<TWorkflowResult>(int minVersion, CancellationToken cancellationToken = default)
        {
            return _workflowVersionCollection.SelectWorkflowVersionAsync<TWorkflowResult>(minVersion, null, cancellationToken);
        }

        /// <summary>
        /// Select that latest <see cref="IWorkflowImplementation{IWorkflowImplementation}"/> with a <see cref="IWorkflowImplementationBase.MajorVersion"/>
        /// in the span <paramref name="minVersion"/> to <paramref name="maxVersion"/>.
        /// </summary>
        [Obsolete("Please use SelectImplementationAsync. Obsolete since 2022-02-10")]
        public Task<IWorkflowImplementation> SelectWorkflowVersionAsync(int minVersion, int maxVersion, CancellationToken cancellationToken = default)
        {
            return _workflowVersionCollection.SelectWorkflowVersionAsync(minVersion, maxVersion, cancellationToken);
        }

        /// <summary>
        /// Select that latest <see cref="IWorkflowImplementation{IWorkflowImplementation}"/> with a <see cref="IWorkflowImplementationBase.MajorVersion"/>
        /// in the span <paramref name="minVersion"/> to <paramref name="maxVersion"/>.
        /// </summary>
        public Task<IWorkflowImplementation> SelectImplementationAsync(int minVersion, int maxVersion, CancellationToken cancellationToken = default)
        {
            return _workflowVersionCollection.SelectWorkflowVersionAsync(minVersion, maxVersion, cancellationToken);
        }

        /// <summary>
        /// Select that latest <see cref="IWorkflowImplementation{IWorkflowImplementation}"/> with a <see cref="IWorkflowImplementationBase.MajorVersion"/>
        /// that is greater than or equal to <paramref name="minVersion"/>.
        /// </summary>
        [Obsolete("Please use SelectImplementationAsync. Obsolete since 2022-02-10")]
        public Task<IWorkflowImplementation> SelectWorkflowVersionAsync(int minVersion, CancellationToken cancellationToken = default)
        {
            return _workflowVersionCollection.SelectWorkflowVersionAsync(minVersion, null, cancellationToken);
        }

        /// <summary>
        /// Select that latest <see cref="IWorkflowImplementation{IWorkflowImplementation}"/> with a <see cref="IWorkflowImplementationBase.MajorVersion"/>
        /// that is greater than or equal to <paramref name="minVersion"/>.
        /// </summary>
        public Task<IWorkflowImplementation> SelectImplementationAsync(int minVersion, CancellationToken cancellationToken = default)
        {
            return _workflowVersionCollection.SelectWorkflowVersionAsync(minVersion, null, cancellationToken);
        }

        /// <summary>
        /// Define the values that are invariable over all implementations for one activity
        /// </summary>
        /// <param name="activityFormId"></param>
        /// <param name="title"></param>
        /// <param name="type"></param>
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

        /// <summary>
        /// Get the values for an activity that are invariable over implementations.
        /// </summary>
        /// <param name="activityFormId"></param>
        /// <returns></returns>
        public ActivityDefinition GetActivityDefinition(string activityFormId)
        {
            var id = activityFormId.ToGuidString();
            if (!_activityDefinitions.ContainsKey(id)) return null;
            return _activityDefinitions[id];
        }

        /// <summary>
        /// Obsolete
        /// </summary>
        [Obsolete("Use SelectImplementationAsync. Compilation warning since 2021-11-12. Will be Compilation error after 2021-12-01")]
        public IWorkflowImplementation<TWorkflowResult> SelectWorkflowVersion<TWorkflowResult>(int minVersion, int maxVersion)
        {
            return _workflowVersionCollection.SelectWorkflowVersionAsync<TWorkflowResult>(minVersion, maxVersion).Result;
        }

        /// <summary>
        /// Obsolete
        /// </summary>
        [Obsolete("Use SelectImplementationAsync. Compilation warning since 2021-11-12. Will be Compilation error after 2021-12-01")]
        public IWorkflowImplementation SelectWorkflowVersion(int minVersion, int maxVersion)
        {
            return _workflowVersionCollection.SelectWorkflowVersionAsync(minVersion, maxVersion).Result;
        }

        /// <inheritdoc />
        public override string ToString() => $"{WorkflowCapabilityName}.{WorkflowFormTitle}";
    }
}