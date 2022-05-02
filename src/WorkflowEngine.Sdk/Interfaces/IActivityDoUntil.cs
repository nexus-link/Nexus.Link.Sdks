using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces;

/// <summary>
/// An activity of type <see cref="ActivityTypeEnum.DoWhileOrUntil"/>.
/// </summary>
public interface IActivityDoUntil : IExecutableActivity
{
    /// <summary>
    /// Continue the loop until <paramref name="conditionMethodAsync"/> return true.
    /// </summary>
    IActivityDoUntil Until(ActivityConditionMethodAsync conditionMethodAsync);

    /// <summary>
    /// Continue the loop until <paramref name="conditionMethod"/> return true.
    /// </summary>
    IActivityDoUntil Until(ActivityConditionMethod conditionMethod);

    /// <summary>
    /// Continue the loop until <paramref name="condition"/> is true.
    /// </summary>
    IActivityDoUntil Until(bool condition);

    /// <summary>
    /// Continue the loop until <paramref name="conditionMethodAsync"/> return true.
    /// </summary>
    IActivityDoUntil While(ActivityConditionMethodAsync conditionMethodAsync);

    /// <summary>
    /// Continue the loop until <paramref name="conditionMethod"/> return true.
    /// </summary>
    IActivityDoUntil While(ActivityConditionMethod conditionMethod);

    /// <summary>
    /// Continue the loop until <paramref name="condition"/> is true.
    /// </summary>
    IActivityDoUntil While(bool condition);
}

/// <summary>
/// An activity of type <see cref="ActivityTypeEnum.DoWhileOrUntil"/>.
/// </summary>
public interface IActivityDoUntil<TActivityReturns> : IExecutableActivity<TActivityReturns>
{
    /// <summary>
    /// Continue the loop until <paramref name="conditionMethodAsync"/> return true.
    /// </summary>
    IActivityDoUntil<TActivityReturns> Until(ActivityConditionMethodAsync conditionMethodAsync);

    /// <summary>
    /// Continue the loop until <paramref name="conditionMethod"/> return true.
    /// </summary>
    IActivityDoUntil<TActivityReturns> Until(ActivityConditionMethod conditionMethod);

    /// <summary>
    /// Continue the loop until <paramref name="condition"/> is true.
    /// </summary>
    IActivityDoUntil<TActivityReturns> Until(bool condition);

    /// <summary>
    /// Continue the loop until <paramref name="conditionMethodAsync"/> return true.
    /// </summary>
    IActivityDoUntil<TActivityReturns> While(ActivityConditionMethodAsync conditionMethodAsync);

    /// <summary>
    /// Continue the loop until <paramref name="conditionMethod"/> return true.
    /// </summary>
    IActivityDoUntil<TActivityReturns> While(ActivityConditionMethod conditionMethod);

    /// <summary>
    /// Continue the loop until <paramref name="condition"/> is true.
    /// </summary>
    IActivityDoUntil<TActivityReturns> While(bool condition);
}