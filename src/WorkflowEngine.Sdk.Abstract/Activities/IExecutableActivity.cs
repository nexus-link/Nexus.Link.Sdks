using System.Threading;
using System.Threading.Tasks;

namespace Nexus.Link.WorkflowEngine.Sdk.Abstract.Activities;

/// <summary>
/// An activity that can be executed
/// </summary>
public interface IExecutableActivity : IActivity
{
    /// <summary>
    /// Start the activity and don't proceed until the activity has completed.
    /// </summary>
    Task ExecuteAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Start the activity but we will not block here. Use <see cref="ISpawnedActivity.AwaitAsync"/>
    /// to await the activity.
    /// </summary>
    Task<ISpawnedActivity> SpawnAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// An activity that can be executed
/// </summary>
public interface IExecutableActivity<TActivityReturns> : IActivity
{
    /// <summary>
    /// Start the activity and don't proceed until the activity has completed.
    /// </summary>
    /// <returns>
    /// The result from the completed activity.
    /// </returns>

    Task<TActivityReturns> ExecuteAsync(CancellationToken cancellationToken = default);

    /// <summary>>
    /// Start the activity but we will not block here. Use <see cref="ISpawnedActivity{TActivityReturns}.AwaitAsync"/>
    /// to await the activity.
    /// </summary>
    Task<ISpawnedActivity<TActivityReturns>> SpawnAsync(CancellationToken cancellationToken = default);
}