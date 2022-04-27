using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
#pragma warning disable CS0618

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces;

/// <summary>
/// The implementation method for an action activity with a return value.
/// </summary>
/// <param name="activity">The current <see cref="IActivityCondition{TActivityReturns}"/>.</param>
/// <param name="cancellationToken"></param>
/// <typeparam name="TMethodReturns">The type of the returned value from the method</typeparam>
public delegate Task<TMethodReturns> ActivityConditionMethodAsync<TMethodReturns>(IActivityCondition<TMethodReturns> activity, CancellationToken cancellationToken);

/// <summary>
/// The implementation method for an action activity with a return value.
/// </summary>
/// <param name="activity">The current <see cref="IActivityCondition{TActivityReturns}"/>.</param>
/// <typeparam name="TMethodReturns">The type of the returned value from the method</typeparam>
public delegate TMethodReturns ActivityConditionMethod<TMethodReturns>(IActivityCondition<TMethodReturns> activity);

/// <summary>
/// An activity of type <see cref="ActivityTypeEnum.Condition"/>.
/// </summary>
[Obsolete("Please use IActivityIf. Obsolete since 2022-04-27.")]
public interface IActivityCondition<TActivityReturns> : IActivity
{
    /// <summary>
    /// The logic to calculate the condition value
    /// </summary>
    Task<TActivityReturns> ExecuteAsync(ActivityConditionMethodAsync<TActivityReturns> methodAsync, CancellationToken cancellationToken = default);

    /// <summary>
    /// The logic to calculate the condition value
    /// </summary>
    Task<TActivityReturns> ExecuteAsync(ActivityConditionMethod<TActivityReturns> method, CancellationToken cancellationToken = default);
}