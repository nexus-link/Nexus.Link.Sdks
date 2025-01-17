using System.Threading;
using System.Threading.Tasks;

namespace Nexus.Link.WorkflowEngine.Sdk.Abstract.Activities;

/// <summary>
/// A spawned activity <see cref="IExecutableActivity.SpawnAsync"/> that should be awaited.
/// </summary>
public interface ISpawnedActivity
{
    /// <summary>
    /// Will block until the activity has completed.
    /// </summary>
    Task AwaitAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// A spawned activity <see cref="IExecutableActivity{TActivityReturns}.SpawnAsync"/> that should be awaited.
/// </summary>
public interface ISpawnedActivity<TActivityReturns>
{
    /// <summary>
    /// Will block until the activity has completed.
    /// </summary>
    Task<TActivityReturns> AwaitAsync(CancellationToken cancellationToken = default);
}