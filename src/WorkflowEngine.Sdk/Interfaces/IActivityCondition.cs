using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces;

/// <summary>
/// An activity of type <see cref="ActivityTypeEnum.Condition"/>.
/// </summary>
[Obsolete("Please use IActivityIf. Obsolete since 2022-04-27.")]
public interface IActivityCondition<TActivityReturns> : IActivity
{
    /// <summary>
    /// The logic to calculate the condition value
    /// </summary>
    Task<TActivityReturns> ExecuteAsync(ActivityMethodAsync<IActivityCondition<TActivityReturns>, TActivityReturns> methodAsync, CancellationToken cancellationToken = default);

    /// <summary>
    /// The logic to calculate the condition value
    /// </summary>
    Task<TActivityReturns> ExecuteAsync(ActivityMethod<IActivityCondition<TActivityReturns>, TActivityReturns> method, CancellationToken cancellationToken = default);
}