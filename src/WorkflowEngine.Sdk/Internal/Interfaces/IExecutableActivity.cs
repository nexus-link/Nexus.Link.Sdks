using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;

/// <summary>
/// An activity that can either be r
/// </summary>
public interface IExecutableActivity : IActivity
{
    /// <summary>
    /// Start the activity and don't proceed until the activity has completed.
    /// </summary>
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}
/// <summary>
/// An activity that can either be r
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
}