using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Services;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Json;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Logic
{
    /// <inheritdoc cref="IActivitySemaphore" />
    internal class ActivitySemaphore : Activity, IActivitySemaphore
    {
        private readonly Dictionary<string, ActivitySemaphore> _raisedActivitySemaphores = new();

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
            _semaphoreService = WorkflowInformation.WorkflowCapabilities.StateCapability.WorkflowSemaphore;
        }

        /// <inheritdoc />
        public async Task RaiseAsync(TimeSpan expiresAfter, CancellationToken cancellationToken = default)
        {
            if (Instance.HasCompleted && Instance.State == ActivityStateEnum.Success)
            {
                var raised = Instance.ResultAsJson != null && JsonHelper.SafeDeserializeObject<bool>(Instance.ResultAsJson);
                if (raised)
                {
                    // Extend the expiration time
                    await ExtendExpirationAsync(expiresAfter, cancellationToken);
                }
            }
            await InternalExecuteAsync((a, ct) => InternalRaiseAsync(expiresAfter, ct), cancellationToken);
            _raisedActivitySemaphores[CalculatedKey] = this;
        }

        private string CalculatedKey => $"{WorkflowInformation.InstanceId}.{ResourceIdentifier}";

        private async Task ExtendExpirationAsync(TimeSpan expiresAfter, CancellationToken cancellationToken)
        {
            var semaphore = new WorkflowSemaphoreCreate
            {
                WorkflowFormId = WorkflowInformation.FormId,
                ResourceIdentifier = ResourceIdentifier,
                WorkflowInstanceId = WorkflowInstanceId,
                ExpiresAt = DateTimeOffset.UtcNow.Add(expiresAfter),
                Raised = true
            };
            try
            {
                await _semaphoreService.UpdateExpirationAsync(semaphore.WorkflowFormId, semaphore.ResourceIdentifier, semaphore.WorkflowInstanceId,
                    semaphore.ExpiresAt, cancellationToken);
            }
            catch (Exception e) when
                (e is FulcrumConflictException or FulcrumNotFoundException)
            {
                throw new ActivityException(ActivityExceptionCategoryEnum.TechnicalError,
                    $"The semaphore {semaphore} was lost for workflow instance {WorkflowInstanceId}.",
                    $"The workflow state has been jeopardized, due to a conflict with another workflow.");
            }
        }

        /// <inheritdoc />
        public Task LowerAsync(CancellationToken cancellationToken = default)
        {
            return InternalExecuteAsync((a, ct) => InternalLowerAsync(ct), cancellationToken);
        }

        private async Task InternalRaiseAsync(TimeSpan expiresAfter, CancellationToken cancellationToken)
        {
            var semaphoreCreate = new WorkflowSemaphoreCreate
            {
                WorkflowFormId = WorkflowInformation.FormId,
                WorkflowInstanceId = WorkflowInstanceId,
                ResourceIdentifier = ResourceIdentifier,
                ExpiresAt = DateTimeOffset.UtcNow.Add(expiresAfter),
                Raised = true
            };
            await _semaphoreService.CreateOrTakeOverOrEnqueueAsync(semaphoreCreate, cancellationToken);
            Instance.ResultAsJson = JsonConvert.SerializeObject(true);
        }

        private async Task InternalLowerAsync(CancellationToken cancellationToken)
        {
            string nextWorkflowInstanceId;
            try
            {
                nextWorkflowInstanceId = await _semaphoreService.LowerAndReturnNextWorkflowInstanceAsync(WorkflowInformation.FormId, ResourceIdentifier, WorkflowInstanceId, cancellationToken);
            }
            catch (Exception e) when
                (e is FulcrumConflictException or FulcrumNotFoundException)
            {
                throw new ActivityException(ActivityExceptionCategoryEnum.TechnicalError,
                    $"The semaphore {ResourceIdentifier} has expired for workflow instance {WorkflowInstanceId}.",
                    $"The workflow state has been jeopardized, due to a conflict with another workflow.");
            }

            // Trigger the next workflow instance
            await WorkflowInformation.WorkflowCapabilities.RequestMgmtCapability.Request.RetryAsync(nextWorkflowInstanceId, cancellationToken);
            var success = _raisedActivitySemaphores.TryGetValue(CalculatedKey, out var activity);
            FulcrumAssert.IsTrue(success, CodeLocation.AsString());
            FulcrumAssert.IsNotNull(activity, CodeLocation.AsString());
            activity!.Instance.ResultAsJson = JsonConvert.SerializeObject(false);
            _raisedActivitySemaphores.Remove(CalculatedKey);
        }
    }
}