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

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Logic
{
    /// <inheritdoc cref="IActivitySemaphore" />
    internal class ActivitySemaphore : Activity, IActivitySemaphore
    {
        private static readonly Dictionary<string, ActivitySemaphore> RaisedActivitySemaphores = new();

        public string ResourceIdentifier { get; }
        private readonly IWorkflowSemaphoreService _semaphoreService;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="activityFlow"></param>
        /// <param name="resourceIdentifier">A string that uniquely identifies the resource that is protected by the semaphore.</param>
        public ActivitySemaphore(IInternalActivityFlow activityFlow, string resourceIdentifier)
            : base(ActivityTypeEnum.Semaphore, activityFlow)
        {
            ResourceIdentifier = resourceIdentifier;
            if (string.IsNullOrWhiteSpace(ResourceIdentifier)) ResourceIdentifier = "";
            _semaphoreService = WorkflowInformation.WorkflowCapabilities.StateCapability.WorkflowSemaphore;
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
                await _semaphoreService.ExtendAsync(semaphoreHolderId, null, cancellationToken);
                lock (RaisedActivitySemaphores)
                {
                    RaisedActivitySemaphores[CalculatedKey] = this;
                }
            }
            await InternalExecuteAsync((a, ct) => InternalRaiseAsync(limit, expiresAfter, ct), _ => Task.FromResult((string)null), cancellationToken);
        }

        private string CalculatedKey => $"{WorkflowInformation.InstanceId}.{ResourceIdentifier}";

        private async Task<string> InternalRaiseAsync(int limit, TimeSpan expiresAfter, CancellationToken cancellationToken)
        {
            InternalContract.RequireGreaterThanOrEqualTo(1, limit, nameof(limit));
            var semaphoreCreate = new WorkflowSemaphoreCreate
            {
                WorkflowFormId = WorkflowInformation.FormId,
                WorkflowInstanceId = WorkflowInstanceId,
                ResourceIdentifier = ResourceIdentifier,
                Limit = limit,
                ExpirationTime = expiresAfter
            };
            var semaphoreHolderId = await _semaphoreService.RaiseAsync(semaphoreCreate, cancellationToken);
            lock (RaisedActivitySemaphores)
            {
                RaisedActivitySemaphores[CalculatedKey] = this;
            }

            return semaphoreHolderId;
        }

        /// <inheritdoc />
        public Task LowerAsync(CancellationToken cancellationToken = default)
        {
            return InternalExecuteAsync((a, ct) => InternalLowerAsync(ct), cancellationToken);
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
            return _semaphoreService.LowerAsync(semaphoreHolderId, cancellationToken);
        }
    }
}