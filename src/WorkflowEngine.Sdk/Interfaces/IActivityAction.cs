using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces;

/// <summary>
/// The implementation method for an action activity with no return value.
/// </summary>
/// <param name="activity">The current <see cref="IActivityAction"/>.</param>
/// <param name="cancellationToken"></param>
/// <returns></returns>
public delegate Task ActivityActionMethodAsync(IActivityAction activity, CancellationToken cancellationToken);

/// <summary>
/// The implementation method for an action activity with a return value.
/// </summary>
/// <param name="activity">The current <see cref="IActivityAction{TMethodReturns}"/>.</param>
/// <param name="cancellationToken"></param>
/// <typeparam name="TMethodReturns">The type of the returned value from the method</typeparam>
public delegate Task<TMethodReturns> ActivityActionMethodAsync<TMethodReturns>(IActivityAction<TMethodReturns> activity, CancellationToken cancellationToken);

/// <summary>
/// An activity of type <see cref="ActivityTypeEnum.Action"/>.
/// </summary>
public interface IActivityAction : IActivity
{
    /// <summary>
    /// Execute the action <paramref name="methodAsync"/>.
    /// </summary>
    Task ExecuteAsync(ActivityActionMethodAsync methodAsync, CancellationToken cancellationToken = default);
}
/// <summary>
/// An activity of type <see cref="ActivityTypeEnum.Action"/>.
/// </summary>
public interface IActivityAction<TActivityReturns> : IActivity
{
    /// <summary>
    /// Execute the action <paramref name="methodAsync"/>.
    /// </summary>
    Task<TActivityReturns> ExecuteAsync(ActivityActionMethodAsync<TActivityReturns> methodAsync, CancellationToken cancellationToken = default);
}