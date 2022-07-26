using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces;

/// <summary>
/// An activity of type <see cref="ActivityTypeEnum.Action"/>.
/// </summary>
public interface IActivityAction : ITryCatchActivity
{
    /// <summary>
    /// Execute the action <paramref name="methodAsync"/>.
    /// </summary>
    [Obsolete("Please use the ExecuteAsync() method without a method in concert with Action(method). Obsolete since 2022-05-01.")]
    Task ExecuteAsync(ActivityMethodAsync<IActivityAction> methodAsync, CancellationToken cancellationToken = default);
}

/// <summary>
/// An activity of type <see cref="ActivityTypeEnum.Action"/>.
/// </summary>
public interface IActivityAction<TActivityReturns> : ITryCatchActivity<TActivityReturns>
{
    /// <summary>
    /// Execute the action <paramref name="methodAsync"/>.
    /// </summary>
    [Obsolete("Please use the ExecuteAsync() method without a method in concert with Action(method). Obsolete since 2022-05-01.")]
    Task<TActivityReturns> ExecuteAsync(ActivityMethodAsync<IActivityAction<TActivityReturns>, TActivityReturns> methodAsync, CancellationToken cancellationToken = default);
}