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
    /// The identifier of the resource that should be locked or needs throttling
    /// </summary>
    string ResourceIdentifier { get; }

    /// <summary>
    /// The maximum number of concurrent workflows over the resource.
    /// </summary>
    int Limit { get; }

    /// <summary>
    /// Raise a semaphore
    /// </summary>
    Task<string> RaiseAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Lower a semaphore
    /// </summary>
    Task LowerAsync(CancellationToken cancellationToken);
}