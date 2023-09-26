using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Execution;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Support
{
    internal class WorkflowVersionCollection
    {
        public IWorkflowContainer WorkflowContainer { get; }
        public IWorkflowEngineRequiredCapabilities WorkflowCapabilities => WorkflowContainer.WorkflowCapabilities;

        public readonly Dictionary<int, IWorkflowImplementationBase> Versions = new();

        public WorkflowVersionCollection(IWorkflowContainer workflowContainer)
        {
            WorkflowContainer = workflowContainer;
        }

        public async Task<IWorkflowImplementation<TResponse>> SelectWorkflowVersionAsync<TResponse>(int minVersion, int? maxVersion = null, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireGreaterThanOrEqualTo(0, minVersion, nameof(minVersion));
            ThrowIfNotManagedAsynchronousRequest();
            var majorVersion = await SelectMajorVersionAsync(minVersion, maxVersion, cancellationToken);
            FulcrumAssert.IsTrue(Versions.ContainsKey(majorVersion), CodeLocation.AsString());
            var implementationCandidate = Versions[majorVersion];
            FulcrumAssert.IsNotNull(implementationCandidate, CodeLocation.AsString());
            InternalContract.Require(majorVersion >= minVersion,
                $"The selected version, {majorVersion}, is not >= {nameof(minVersion)} ({minVersion})."
                + $" Probably because the current running workflow {implementationCandidate} {FulcrumApplication.Context.ExecutionId} is out of bounds.");
            if (maxVersion.HasValue)
            {
                InternalContract.Require(majorVersion <= maxVersion.Value,
                $"The selected version, {majorVersion}, is not <= {nameof(maxVersion)} ({maxVersion.Value})."
                + $" Probably because the current running workflow {implementationCandidate} {FulcrumApplication.Context.ExecutionId} is out of bounds.");
            }
            var implementation = implementationCandidate as IWorkflowImplementation<TResponse>;
            InternalContract.Require(implementation != null,
                $"The implementation {implementationCandidate} does not implement {nameof(IWorkflowImplementation<TResponse>)}.");
            return implementation!.CreateWorkflowInstance();
        }

        public async Task<IWorkflowImplementation> SelectWorkflowVersionAsync(int minVersion, int? maxVersion = null, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireGreaterThanOrEqualTo(0, minVersion, nameof(minVersion));
            ThrowIfNotManagedAsynchronousRequest();
            var majorVersion = await SelectMajorVersionAsync(minVersion, maxVersion, cancellationToken);
            FulcrumAssert.IsTrue(Versions.ContainsKey(majorVersion), CodeLocation.AsString());

            var implementationCandidate = Versions[majorVersion];
            var implementation = implementationCandidate as IWorkflowImplementation;
            InternalContract.Require(implementation != null,
                $"The implementation {implementationCandidate} does not implement {nameof(IWorkflowImplementation)}.");
            return implementation!.CreateWorkflowInstance();
        }

        private static void ThrowIfNotManagedAsynchronousRequest()
        {
            if (string.IsNullOrWhiteSpace(FulcrumApplication.Context.ManagedAsynchronousRequestId))
            {
                throw new ActivityPostponedException(TimeSpan.Zero);
            }
        }

        private async Task<int> SelectMajorVersionAsync(int minVersion, int? maxVersion = null, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireGreaterThanOrEqualTo(0, minVersion, nameof(minVersion));
            var workflowVersion = await GetWorkflowVersionAsync(cancellationToken);
            int majorVersion;
            if (workflowVersion == null)
            {
                if (maxVersion.HasValue)
                {
                    majorVersion = maxVersion.Value;
                    InternalContract.Require(Versions.ContainsKey(majorVersion),
                        $"Could not find an implementation for the parameter {nameof(maxVersion)} = {maxVersion}");
                }
                else
                {
                    majorVersion = Versions.Keys.Max();
                }
            }
            else
            {
                majorVersion = workflowVersion.MajorVersion;
                InternalContract.Require(Versions.ContainsKey(majorVersion),
                    $"Could not find an implementation for the current work flow instance with id = {FulcrumApplication.Context.ExecutionId}, which has major version = {majorVersion}");
            }

            return majorVersion;
        }

        private async Task<WorkflowVersion> GetWorkflowVersionAsync(CancellationToken cancellationToken = default)
        {
            FulcrumAssert.IsNotNullOrWhiteSpace(FulcrumApplication.Context.ExecutionId, CodeLocation.AsString());
            var workflowInstanceId = FulcrumApplication.Context.ExecutionId;
            var workflowInstance =
                await WorkflowContainer.WorkflowCapabilities.StateCapability.WorkflowInstance.ReadAsync(workflowInstanceId,
                    cancellationToken);
            if (workflowInstance == null) return null;
            var workflowVersion = await WorkflowContainer.WorkflowCapabilities.ConfigurationCapability.WorkflowVersion.ReadAsync(workflowInstance.WorkflowVersionId, cancellationToken);
            FulcrumAssert.IsNotNull(workflowVersion, CodeLocation.AsString());
            return workflowVersion;
        }

        public WorkflowVersionCollection AddWorkflowVersion(IWorkflowImplementationBase workflowImplementation)
        {
            Versions.Add(workflowImplementation.MajorVersion, workflowImplementation);
            return this;
        }
    }
}