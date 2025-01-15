using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Component.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Abstract.Component.Services;

/// <summary>
/// Management methods for an activity
/// </summary>
public interface IActivityService
{
    /// <summary>
    /// Mark an activity as successful with result <paramref name="result"/>.
    /// </summary>
    Task SuccessAsync(string id, ActivitySuccessResult result, CancellationToken cancellationToken = default);

    /// <summary>
    /// Mark an activity as failed with result <paramref name="result"/>.
    /// </summary>
    Task FailedAsync(string id, ActivityFailedResult result, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retry an activity that is in a halted state
    /// </summary>
    Task RetryAsync(string activityInstanceId, CancellationToken cancellationToken = default);
}