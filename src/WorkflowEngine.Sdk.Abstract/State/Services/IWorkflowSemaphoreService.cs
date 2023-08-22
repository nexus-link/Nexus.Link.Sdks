using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Services;

/// <summary>
/// Methods for handling a <see cref="WorkflowSemaphoreCreate"/>.
/// </summary>
public interface IWorkflowSemaphoreService
{
    /// <summary>
    /// Raise a workflow semaphore
    /// </summary>
    /// <param name="request">The values that we would like to use for the lock.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>
    /// The id of the semaphore holder.
    /// </returns>
    /// <exception cref="RequestPostponedException">
    /// The request has been queued.
    /// </exception>
    /// <exception cref="FulcrumConflictException">
    /// The semaphore has expired and another instance is now owning the semaphore.
    /// </exception>
    Task<string> RaiseAsync(WorkflowSemaphoreCreate request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Extend the expiration time of the semaphore.
    /// </summary>
    /// <param name="semaphoreHolderId"></param>
    /// <param name="timeToExpiration">How long time from now should we extend the expiration to? Null means use the last value.</param>
    /// <param name="cancellationToken"></param>
    /// <exception cref="FulcrumConflictException">
    /// The semaphore has expired and another instance is now owning the semaphore.
    /// </exception>
    /// <exception cref="FulcrumTryAgainException">
    /// We could not update the semaphore information right now.
    /// </exception>
    Task ExtendAsync(string semaphoreHolderId, TimeSpan? timeToExpiration, CancellationToken cancellationToken);

    /// <summary>
    /// Lower the workflow semaphore
    /// </summary>
    /// <param name="semaphoreHolderId">The id of the holder of the semaphore.</param>
    /// <param name="cancellationToken"></param>
    /// <exception cref="FulcrumConflictException">
    /// The semaphore has expired and another instance is now owning the semaphore.
    /// </exception>
    Task LowerAsync(string semaphoreHolderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lower all semaphores for the workflow instance.
    /// </summary>
    /// <param name="workflowInstanceId">The id of the workflow instance that should have all its semaphores lowered.</param>
    /// <param name="cancellationToken"></param>
    /// <exception cref="FulcrumConflictException">
    /// The semaphore has expired and another instance is now owning the semaphore.
    /// </exception>
    /// <remarks>
    /// This service should be called when the instance is successful or has failed.
    /// </remarks>
    Task LowerAllAsync(string workflowInstanceId, CancellationToken cancellationToken = default);
}