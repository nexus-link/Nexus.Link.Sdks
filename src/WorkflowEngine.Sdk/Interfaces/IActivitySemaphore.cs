using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces;

/// <summary>
/// An activity of type <see cref="ActivityTypeEnum.Semaphore"/>.
/// </summary>
public interface IActivitySemaphore : IActivity
{
    /// <summary>
    /// Raise a semaphore
    /// </summary>
    /// <param name="expiresAfter">How long time can we hold the semaphore?</param>
    /// <param name="cancellationToken"></param>
    Task RaiseAsync(TimeSpan expiresAfter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Raise a semaphore with a <paramref name="limit"/> of the number of concurrent executions.
    /// </summary>
    /// <param name="limit">The maximum number of concurrent executions.</param>
    /// <param name="expiresAfter">How long time can we hold the semaphore?</param>
    /// <param name="cancellationToken"></param>
    Task RaiseWithLimitAsync(int limit, TimeSpan expiresAfter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lower a semaphore
    /// </summary>
    /// <param name="cancellationToken"></param>
    Task LowerAsync(CancellationToken cancellationToken = default);
}