﻿using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces;

/// <summary>
/// The condition method for the if activity. If it returns true, the then-method is called, otherwise the else-method is called.
/// </summary>
/// <param name="activity">The current <see cref="IActivityIf"/>.</param>
/// <param name="cancellationToken"></param>
public delegate Task<bool> ActivityIfConditionMethodAsync(IActivity activity, CancellationToken cancellationToken);

/// <summary>
/// The condition method for the if activity. If it returns true, the then-method is called, otherwise the else-method is called.
/// </summary>
/// <param name="activity">The current <see cref="IActivityIf"/>.</param>
public delegate bool ActivityIfConditionMethod(IActivity activity);

/// <summary>
/// An activity of type <see cref="ActivityTypeEnum.If"/>.
/// </summary>
public interface IActivityIf : IExecutableActivity
{
    /// <summary>
    /// The method that decides if we should execute the then-method or the else-method.
    /// </summary>
    ActivityIfConditionMethodAsync ConditionMethodAsync { get; }

    /// <summary>
    /// Declare that <paramref name="methodAsync"/> should be executed if <see cref="ConditionMethodAsync"/> returns true.
    /// </summary>
    IActivityIf Then(ActivityMethodAsync<IActivityIf> methodAsync);

    /// <summary>
    /// Declare that <paramref name="method"/> should be executed if <see cref="ConditionMethodAsync"/> returns true.
    /// </summary>
    IActivityIf Then(ActivityMethod<IActivityIf> method);

    /// <summary>
    /// Declare that <paramref name="methodAsync"/> should be executed if <see cref="ConditionMethodAsync"/> returns false.
    /// </summary>
    IActivityIf Else(ActivityMethodAsync<IActivityIf> methodAsync);

    /// <summary>
    /// Declare that <paramref name="method"/> should be executed if <see cref="ConditionMethodAsync"/> returns false.
    /// </summary>
    IActivityIf Else(ActivityMethod<IActivityIf> method);
}
/// <summary>
/// An activity of type <see cref="ActivityTypeEnum.If"/>.
/// </summary>
public interface IActivityIf<TActivityReturns> :
    IExecutableActivity<TActivityReturns>
{
    /// <summary>
    /// The method that decides if we should execute the then-method or the else-method.
    /// </summary>
    ActivityIfConditionMethodAsync ConditionMethodAsync { get; }

    /// <summary>
    /// Declare that <paramref name="methodAsync"/> should be executed if <see cref="ConditionMethodAsync"/> returns true.
    /// </summary>
    IActivityIf<TActivityReturns> Then(ActivityMethodAsync<IActivityIf<TActivityReturns>, TActivityReturns> methodAsync);

    /// <summary>
    /// Declare that <paramref name="method"/> should be executed if <see cref="ConditionMethodAsync"/> returns true.
    /// </summary>
    IActivityIf<TActivityReturns> Then(ActivityMethod<IActivityIf<TActivityReturns>, TActivityReturns> method);

    /// <summary>
    /// Declare that <paramref name="value"/> should be returned if <see cref="ConditionMethodAsync"/> returns true.
    /// </summary>
    IActivityIf<TActivityReturns> Then(TActivityReturns value);

    /// <summary>
    /// Declare that <paramref name="methodAsync"/> should be executed if <see cref="ConditionMethodAsync"/> returns false.
    /// </summary>
    IActivityIf<TActivityReturns> Else(ActivityMethodAsync<IActivityIf<TActivityReturns>, TActivityReturns> methodAsync);

    /// <summary>
    /// Declare that <paramref name="method"/> should be executed if <see cref="ConditionMethodAsync"/> returns false.
    /// </summary>
    IActivityIf<TActivityReturns> Else(ActivityMethod<IActivityIf<TActivityReturns>, TActivityReturns> method);

    /// <summary>
    /// Declare that <paramref name="value"/> should be returned if <see cref="ConditionMethodAsync"/> returns false.
    /// </summary>
    IActivityIf<TActivityReturns> Else(TActivityReturns value);
}