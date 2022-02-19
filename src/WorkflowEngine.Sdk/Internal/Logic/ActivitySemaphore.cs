using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Services;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Json;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Logic
{
    /// <inheritdoc cref="IActivitySemaphore" />
    internal class ActivitySemaphore : Activity, IActivitySemaphore
    {
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
                var semaphoreId = JsonHelper.SafeDeserializeObject<string>(Instance.ResultAsJson);
                // Extend the expiration time
                await ExtendExpirationAsync(semaphoreId, expiresAfter, cancellationToken);
            }
            await InternalExecuteAsync((a, ct) => InternalRaiseAsync(expiresAfter, ct), cancellationToken);
        }

        private async Task ExtendExpirationAsync(string semaphoreId, TimeSpan expiresAfter, CancellationToken cancellationToken)
        {
            try
            {
                await _semaphoreService.UpdateExpirationAsync(semaphoreId, WorkflowInstanceId,
                    DateTimeOffset.UtcNow.Add(expiresAfter),
                    cancellationToken);
            }
            catch (Exception e) when
                (e is FulcrumConflictException or FulcrumNotFoundException)
            {
                throw new ActivityException(ActivityExceptionCategoryEnum.TechnicalError,
                    $"The semaphore {semaphoreId} was lost for workflow instance {WorkflowInstanceId}.",
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
        }
    }
}