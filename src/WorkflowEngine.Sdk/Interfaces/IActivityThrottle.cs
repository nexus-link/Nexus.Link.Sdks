using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces;

/// <summary>
/// An activity of type <see cref="ActivityTypeEnum.Throttle"/>, i.e. an activity where we try to reduce the number of workflow instances that access the same resource.
/// The throttling is active over all workflow forms.
/// </summary>
public interface IActivityThrottle : IActivity
{
    /// <summary>
    /// The identifier of the resource that needs throttling
    /// </summary>
    string ResourceIdentifier { get; }

    /// <summary>
    /// The maximum number of concurrent workflows over the resource.
    /// </summary>
    int Limit { get; }

    /// <summary>
    /// Raise a semaphore with a maximum number of concurrent executions, execute the <paramref name="methodAsync"/>, lower the semaphore.
    /// </summary>
    Task ExecuteAsync(ActivityMethodAsync<IActivityThrottle> methodAsync, CancellationToken cancellationToken = default);
}

/// <summary>
/// An activity of type <see cref="ActivityTypeEnum.Throttle"/>, i.e. an activity where we try to reduce the number of workflow instances that access the same resource.
/// </summary>
public interface IActivityThrottle<TActivityReturns> : IActivity
{
    /// <summary>
    /// The identifier of the resource that needs throttling
    /// </summary>
    string ResourceIdentifier { get; }

    /// <summary>
    /// The maximum number of concurrent workflows over the resource.
    /// </summary>
    int Limit { get; }

    /// <summary>
    /// Raise a semaphore with a maximum number of concurrent executions, execute the <paramref name="methodAsync"/>, lower the semaphore.
    /// </summary>
    Task<TActivityReturns> ExecuteAsync(ActivityMethodAsync<IActivityThrottle<TActivityReturns>, TActivityReturns> methodAsync, CancellationToken cancellationToken = default);
}