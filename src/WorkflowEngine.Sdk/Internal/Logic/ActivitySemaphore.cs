using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Services;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Json;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Support;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Logic
{
    /// <inheritdoc cref="IActivitySemaphore" />
    internal class ActivitySemaphore : Activity, IActivitySemaphore
    {
        private static readonly Dictionary<string, ActivitySemaphore> RaisedActivitySemaphores = new();

        public string ResourceIdentifier { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="activityInformation"></param>
        /// <param name="resourceIdentifier">A string that uniquely identifies the resource that is protected by the semaphore.</param>
        public ActivitySemaphore(IActivityInformation activityInformation, string resourceIdentifier)
            : base(activityInformation)
        {
            InternalContract.Require(ActivityInformation.Workflow.SemaphoreService != null, $"You must provide a {typeof(IWorkflowSemaphoreService)}.");
            ResourceIdentifier = resourceIdentifier;
            if (string.IsNullOrWhiteSpace(ResourceIdentifier)) ResourceIdentifier = "";
        }

        /// <inheritdoc />
        public Task RaiseAsync(TimeSpan expiresAfter, CancellationToken cancellationToken = default)
        {
            return RaiseWithLimitAsync(1, expiresAfter, cancellationToken);
        }

        /// <inheritdoc />
        public async Task RaiseWithLimitAsync(int limit, TimeSpan expiresAfter, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireGreaterThanOrEqualTo(1, limit, nameof(limit));
            if (Instance.HasCompleted && Instance.State == ActivityStateEnum.Success)
            {
                if (Instance.ResultAsJson == null)
                {
                    // The workflow has reached the point where the semaphore has been lowered.
                    return;
                }
                var semaphoreHolderId = JsonHelper.SafeDeserializeObject<string>(Instance.ResultAsJson);
                if (semaphoreHolderId == null)
                {
                    // The semaphore has been lowered
                    return;
                }
                await ActivityInformation.Workflow.SemaphoreService.ExtendAsync(semaphoreHolderId, null, cancellationToken);
                lock (RaisedActivitySemaphores)
                {
                    RaisedActivitySemaphores[CalculatedKey] = this;
                }
            }
            await ActivityExecutor.ExecuteWithReturnValueAsync(
                ct => InternalRaiseAsync(limit, expiresAfter, ct),
                _ => Task.FromResult((string)null),
                cancellationToken);
        }

        private string CalculatedKey => $"{WorkflowInstanceId}.{ResourceIdentifier}";

        private async Task<string> InternalRaiseAsync(int limit, TimeSpan expiresAfter, CancellationToken cancellationToken)
        {
            InternalContract.RequireGreaterThanOrEqualTo(1, limit, nameof(limit));
            var semaphoreCreate = new WorkflowSemaphoreCreate
            {
                WorkflowFormId = ActivityInformation.Workflow.FormId,
                WorkflowInstanceId = WorkflowInstanceId,
                ResourceIdentifier = ResourceIdentifier,
                Limit = limit,
                ExpirationTime = expiresAfter
            };
            var semaphoreHolderId = await ActivityInformation.Workflow.SemaphoreService.RaiseAsync(semaphoreCreate, cancellationToken);
            lock (RaisedActivitySemaphores)
            {
                RaisedActivitySemaphores[CalculatedKey] = this;
            }

            return semaphoreHolderId;
        }

        /// <inheritdoc />
        public Task LowerAsync(CancellationToken cancellationToken = default)
        {
            return ActivityExecutor.ExecuteWithoutReturnValueAsync(InternalLowerAsync, cancellationToken);
        }

        private Task InternalLowerAsync(CancellationToken cancellationToken)
        {
            string semaphoreHolderId;
            lock (RaisedActivitySemaphores)
            {
                var found = RaisedActivitySemaphores.TryGetValue(CalculatedKey, out var activity);
                FulcrumAssert.IsTrue(found, CodeLocation.AsString());
                FulcrumAssert.IsNotNull(activity!.Instance.ResultAsJson, CodeLocation.AsString());
                semaphoreHolderId = JsonHelper.SafeDeserializeObject<string>(activity!.Instance.ResultAsJson);
                FulcrumAssert.IsNotNullOrWhiteSpace(semaphoreHolderId, CodeLocation.AsString());
                activity!.Instance.ResultAsJson = JsonConvert.SerializeObject(null);
                RaisedActivitySemaphores.Remove(CalculatedKey);
            }
            return ActivityInformation.Workflow.SemaphoreService.LowerAsync(semaphoreHolderId, cancellationToken);
        }
    }
}