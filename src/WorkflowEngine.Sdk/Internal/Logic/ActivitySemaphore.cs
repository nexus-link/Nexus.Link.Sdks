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
        private readonly IWorkflowSemaphoreService _semaphoreService;

        public ActivitySemaphore(IInternalActivityFlow activityFlow)
            : base(ActivityTypeEnum.Semaphore, activityFlow)
        {
            _semaphoreService = WorkflowInformation.WorkflowCapabilities.StateCapability.WorkflowSemaphore;
        }

        /// <inheritdoc />
        public async Task RaiseAsync(string resourceIdentifier, TimeSpan expiresAfter, CancellationToken cancellationToken = default)
        {
            if (Instance.HasCompleted && Instance.State == ActivityStateEnum.Success)
            {
                var semaphoreId = JsonHelper.SafeDeserializeObject<string>(Instance.ResultAsJson);
                // Extend the expiration time
                await ExtendExpirationAsync(semaphoreId, expiresAfter, cancellationToken);
            }
            await InternalExecuteAsync((a, ct) => InternalRaiseAsync(resourceIdentifier, expiresAfter, ct), cancellationToken);
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
        public Task LowerAsync(string semaphoreId, CancellationToken cancellationToken = default)
        {
            return InternalExecuteAsync((a, ct) => InternalLowerAsync(semaphoreId, ct), cancellationToken);
        }

        private async Task InternalRaiseAsync(string resourceIdentifier, TimeSpan expiresAfter, CancellationToken cancellationToken)
        {
            var semaphoreCreate = new WorkflowSemaphoreCreate
            {
                WorkflowFormId = WorkflowInformation.FormId,
                WorkflowInstanceId = WorkflowInstanceId,
                ResourceIdentifier = resourceIdentifier,
                ExpiresAt = DateTimeOffset.UtcNow.Add(expiresAfter),
                Raised = true
            };
            await _semaphoreService.CreateOrTakeOverOrEnqueueAsync(semaphoreCreate, cancellationToken);
        }

        private async Task InternalLowerAsync(string resourceIdentifier, CancellationToken cancellationToken)
        {
            string nextWorkflowInstanceId;
            try
            {
                nextWorkflowInstanceId = await _semaphoreService.LowerAndReturnNextWorkflowInstanceAsync(WorkflowInformation.FormId, resourceIdentifier, WorkflowInstanceId, cancellationToken);
            }
            catch (Exception e) when
                (e is FulcrumConflictException or FulcrumNotFoundException)
            {
                throw new ActivityException(ActivityExceptionCategoryEnum.TechnicalError,
                    $"The semaphore {resourceIdentifier} has expired for workflow instance {WorkflowInstanceId}.",
                    $"The workflow state has been jeopardized, due to a conflict with another workflow.");
            }

            // Trigger the next workflow instance
            await WorkflowInformation.WorkflowCapabilities.RequestMgmtCapability.Request.RetryAsync(nextWorkflowInstanceId, cancellationToken);
        }
    }
}