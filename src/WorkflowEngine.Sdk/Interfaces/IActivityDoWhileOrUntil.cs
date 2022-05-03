using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces;

/// <summary>
/// An activity of type <see cref="ActivityTypeEnum.DoWhileOrUntil"/>.
/// </summary>
public interface IActivityDoWhileOrUntil : IExecutableActivity, ILoopActivity
{
    /// <summary>
    /// Continue the loop until <paramref name="conditionMethodAsync"/> return true.
    /// </summary>
    IActivityDoWhileOrUntil Until(ActivityConditionMethodAsync<IActivityDoWhileOrUntil> conditionMethodAsync);

    /// <summary>
    /// Continue the loop until <paramref name="conditionMethod"/> return true.
    /// </summary>
    IActivityDoWhileOrUntil Until(ActivityConditionMethod<IActivityDoWhileOrUntil> conditionMethod);

    /// <summary>
    /// Continue the loop until <paramref name="condition"/> is true.
    /// </summary>
    IActivityDoWhileOrUntil Until(bool condition);

    /// <summary>
    /// Continue the loop until <paramref name="conditionMethodAsync"/> return true.
    /// </summary>
    IActivityDoWhileOrUntil While(ActivityConditionMethodAsync<IActivityDoWhileOrUntil> conditionMethodAsync);

    /// <summary>
    /// Continue the loop until <paramref name="conditionMethod"/> return true.
    /// </summary>
    IActivityDoWhileOrUntil While(ActivityConditionMethod<IActivityDoWhileOrUntil> conditionMethod);

    /// <summary>
    /// Continue the loop until <paramref name="condition"/> is true.
    /// </summary>
    IActivityDoWhileOrUntil While(bool condition);
}

/// <summary>
/// An activity of type <see cref="ActivityTypeEnum.DoWhileOrUntil"/>.
/// </summary>
public interface IActivityDoWhileOrUntil<TActivityReturns> : IExecutableActivity<TActivityReturns>, ILoopActivity
{
    /// <summary>
    /// Continue the loop until <paramref name="conditionMethodAsync"/> return true.
    /// </summary>
    IActivityDoWhileOrUntil<TActivityReturns> Until(ActivityConditionMethodAsync<IActivityDoWhileOrUntil<TActivityReturns>> conditionMethodAsync);

    /// <summary>
    /// Continue the loop until <paramref name="conditionMethod"/> return true.
    /// </summary>
    IActivityDoWhileOrUntil<TActivityReturns> Until(ActivityConditionMethod<IActivityDoWhileOrUntil<TActivityReturns>> conditionMethod);

    /// <summary>
    /// Continue the loop until <paramref name="condition"/> is true.
    /// </summary>
    IActivityDoWhileOrUntil<TActivityReturns> Until(bool condition);

    /// <summary>
    /// Continue the loop until <paramref name="conditionMethodAsync"/> return true.
    /// </summary>
    IActivityDoWhileOrUntil<TActivityReturns> While(ActivityConditionMethodAsync<IActivityDoWhileOrUntil<TActivityReturns>> conditionMethodAsync);

    /// <summary>
    /// Continue the loop until <paramref name="conditionMethod"/> return true.
    /// </summary>
    IActivityDoWhileOrUntil<TActivityReturns> While(ActivityConditionMethod<IActivityDoWhileOrUntil<TActivityReturns>> conditionMethod);

    /// <summary>
    /// Continue the loop until <paramref name="condition"/> is true.
    /// </summary>
    IActivityDoWhileOrUntil<TActivityReturns> While(bool condition);
}