using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;

internal interface ISemaphoreSupport
{
    /// <summary>
    /// The activity that this semaphore support is associated to.
    /// </summary>
    Activity Activity { get; set; }

    /// <summary>
    /// True if this is a throttle semaphore. False if it is a Lock semaphore.
    /// </summary>
    bool IsThrottle { get; }

    /// <summary>
    /// The identifier of the resource that should be locked or needs throttling
    /// </summary>
    string ResourceIdentifier { get; }

    /// <summary>
    /// The maximum number of concurrent workflows over the resource.
    /// </summary>
    /// <remarks>
    /// Must be 1 for locks.
    /// </remarks>
    int Limit { get; }

    /// <summary>
    /// The time span that the <see cref="Limit"/> applies to. Null means that the <see cref="Limit"/> is for concurrent instances.
    /// </summary>
    /// <remarks>
    /// Must be null for locks.
    /// </remarks>
    TimeSpan? LimitationTimeSpan { get; }

    /// <summary>
    /// Raise a semaphore
    /// </summary>
    Task<string> RaiseAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Lower a semaphore
    /// </summary>
    Task LowerAsync(CancellationToken cancellationToken);
}