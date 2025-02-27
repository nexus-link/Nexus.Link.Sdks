﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Crud.Model;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Execution;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Support;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Log = Nexus.Link.Libraries.Core.Logging.Log;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;

/// <summary>
/// Handles the execution of a workflow. This is only on the top level, it is <see cref="ActivityExecutor"/> that handles the actual activity execution.
/// </summary>
internal class WorkflowBeforeAndAfterExecution : IWorkflowBeforeAndAfterExecution
{
    private readonly MethodHandler _methodHandler;
    private Lock<string> _workflowDistributedLock;
    public IWorkflowInformation WorkflowInformation { get; }

    public WorkflowBeforeAndAfterExecution(IWorkflowInformation workflowInformation)
    {
        WorkflowInformation = workflowInformation;
        _methodHandler = new MethodHandler(WorkflowInformation.FormTitle);
    }

    public async Task BeforeExecutionAsync(CancellationToken cancellationToken)
    {
        try
        {
            await InternalBeforeExecutionAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            if (ex is WorkflowFailedException) throw;
            // All other exceptions should result in a retry
            throw new WorkflowPostponedException(TimeSpan.FromSeconds(30));
        }
    }

    private async Task InternalBeforeExecutionAsync(CancellationToken cancellationToken)
    {
        FulcrumAssert.IsNotNullOrWhiteSpace(FulcrumApplication.Context.ExecutionId, CodeLocation.AsString());
        WorkflowInformation.InstanceId = FulcrumApplication.Context.ExecutionId.ToGuidString();
        if (WorkflowInformation.WorkflowInstanceService != null)
        {
            _workflowDistributedLock = await WorkflowInformation.WorkflowInstanceService.ClaimDistributedLockAsync(
                WorkflowInformation.InstanceId, TimeSpan.FromMinutes(5), null, cancellationToken);
        }

        try
        {
            await WorkflowInformation.LoadAsync(cancellationToken);

            if (WorkflowInformation.Instance.State == WorkflowStateEnum.Executing)
            {
                // This means that we have an execution that didn't suceed in saving its state, we must fail the entire workflow instance
                var technicalMessage =
                    $"The workflow instance {WorkflowInformation.InstanceId} could not save its state. This means that the integrity of the workflow can no longer be guaranteed, and we are forced to fail the workflow.";
                Log.LogError(technicalMessage);
                throw new WorkflowFailedException(ActivityExceptionCategoryEnum.TechnicalError,
                    technicalMessage,
                    $"The workflow could not save its state. This means that the integrity of the workflow can no longer be guaranteed, and we are forced to fail the workflow.");
            }

            WorkflowInformation.Form.CapabilityName = WorkflowInformation.CapabilityName;
            WorkflowInformation.Form.Title = WorkflowInformation.FormTitle;
            WorkflowInformation.Version.MinorVersion = WorkflowInformation.MinorVersion;
            WorkflowInformation.Instance.State = WorkflowStateEnum.Executing;
            WorkflowInformation.Instance.Title = WorkflowInformation.InstanceTitle;
            try
            {
                Log.LogInformation(
                    $"Saving workflow instance state to {WorkflowInformation.Instance.State} for {WorkflowInformation.Instance} ({WorkflowInformation.InstanceId}).");
                await WorkflowInformation.SaveToDbAsync(cancellationToken);
            }
            catch (Exception dbException)
            {
                Log.LogInformation(
                    $"Failed to save workflow instance state to {WorkflowInformation.Instance.State} for {WorkflowInformation.Instance} ({WorkflowInformation.InstanceId}).",
                    dbException);
                throw new ActivityTemporaryFailureException(TimeSpan.FromSeconds(30));
            }
        }
        catch (Exception ex)
        {
            try
            {
                if (WorkflowInformation.WorkflowInstanceService != null && _workflowDistributedLock?.LockId != null)
                {
                    await WorkflowInformation.WorkflowInstanceService.ReleaseDistributedLockAsync(
                        WorkflowInformation.InstanceId, _workflowDistributedLock.LockId, cancellationToken);
                }
            }
            catch (Exception)
            {
                Log.LogWarning($"Could not release the distributed lock for workflow {WorkflowInformation.InstanceId}. It will be released automatically within a couple of minutes.", ex);
                // Ignore
            }
            throw;
        }
    }

    public async Task AfterExecutionAsync(CancellationToken cancellationToken)
    {
        try
        {
            await InternalAfterExecutionAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            var message = $"The workflow {WorkflowInformation} had a problem with the book keeping after execution. Will try again later.\r{ex.ToLog()}";
            Log.LogWarning(message, ex);
            throw new FulcrumTryAgainException(message)
            {
                RecommendedWaitTimeInSeconds = 30
            };
        }
    }

    private async Task InternalAfterExecutionAsync(CancellationToken orginalCancellationToken)
    {
        // Please note! We will not honor the original cancellation token here, because it is very important
        // that we save the state. We will give ourselves at least 30 seconds, no matter what the original
        // cancellation token thinks
        var timeLeftToFinish = CalculateTimeLeftToFinish(TimeSpan.FromSeconds(30));
        var limitedTimeCancellationToken = new CancellationTokenSource(timeLeftToFinish);
        var extendedCancellationToken = limitedTimeCancellationToken.Token;

        try
        {
            WorkflowInformation.AggregateActivityInformation();

            var doAnInitialSaveToFallback = WorkflowInformation.NumberOfActivityInstances > 100;
            if (doAnInitialSaveToFallback)
            {
                Log.LogInformation($"We will do an initial save of the state to fallback storage due to many activity instances ({WorkflowInformation.NumberOfActivityInstances}) for {WorkflowInformation.Instance} ({WorkflowInformation.InstanceId}).");
            }
            await WorkflowInformation.SaveAsync(doAnInitialSaveToFallback, extendedCancellationToken);

            // Release semaphores
            if (WorkflowInformation.Instance.State is not (WorkflowStateEnum.Success or WorkflowStateEnum.Failed)) return;
            try
            {
                await WorkflowInformation.SemaphoreService.LowerAllAsync(WorkflowInformation.InstanceId, extendedCancellationToken);
            }
            catch (Exception ex)
            {
                throw new FulcrumTryAgainException($"Could not lower semaphores: {ex.Message}");
            }
        }
        finally
        {
            if (_workflowDistributedLock != null)
            {
                try
                {
                    // Release the lock
                    FulcrumAssert.IsNotNull(WorkflowInformation.WorkflowInstanceService, CodeLocation.AsString());
                    await WorkflowInformation.WorkflowInstanceService.ReleaseDistributedLockAsync(
                        _workflowDistributedLock.ItemId, _workflowDistributedLock.LockId,
                        extendedCancellationToken);
                }
                catch (Exception)
                {
                    // Never let this cause a failure
                }
            }
        }
    }

    private TimeSpan CalculateTimeLeftToFinish(TimeSpan? minimumTime = null)
    {
        TimeSpan timeLeftToFinish;
        if (FulcrumApplication.IsInDevelopment)
        {
            var remainingTime =
                WorkflowInformation.DefaultActivityOptions.MaxTotalRunTimeSpan.Subtract(WorkflowInformation
                    .TimeSinceCurrentRunStarted.Elapsed);
            timeLeftToFinish = remainingTime;
        }
        else
        {
            timeLeftToFinish = TimeSpan.FromSeconds(100).Subtract(WorkflowInformation.TimeSinceCurrentRunStarted.Elapsed)
                // We need some time to spare
                .Subtract(TimeSpan.FromSeconds(2));
        }

        if (minimumTime.HasValue && timeLeftToFinish < minimumTime.Value) timeLeftToFinish = minimumTime.Value;
        return timeLeftToFinish;
    }
}