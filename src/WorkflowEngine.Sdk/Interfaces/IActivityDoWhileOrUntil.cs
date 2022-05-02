using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces;

/// <summary>
/// An activity of type <see cref="ActivityTypeEnum.DoWhileOrUntil"/>.
/// </summary>
public interface IActivityDoWhileOrUntil : IExecutableActivity
{
    /// <summary>
    /// Continue the loop until <paramref name="conditionMethodAsync"/> return true.
    /// </summary>
    IActivityDoWhileOrUntil Until(ActivityConditionMethodAsync conditionMethodAsync);

    /// <summary>
    /// Continue the loop until <paramref name="conditionMethod"/> return true.
    /// </summary>
    IActivityDoWhileOrUntil Until(ActivityConditionMethod conditionMethod);

    /// <summary>
    /// Continue the loop until <paramref name="condition"/> is true.
    /// </summary>
    IActivityDoWhileOrUntil Until(bool condition);

    /// <summary>
    /// Continue the loop until <paramref name="conditionMethodAsync"/> return true.
    /// </summary>
    IActivityDoWhileOrUntil While(ActivityConditionMethodAsync conditionMethodAsync);

    /// <summary>
    /// Continue the loop until <paramref name="conditionMethod"/> return true.
    /// </summary>
    IActivityDoWhileOrUntil While(ActivityConditionMethod conditionMethod);

    /// <summary>
    /// Continue the loop until <paramref name="condition"/> is true.
    /// </summary>
    IActivityDoWhileOrUntil While(bool condition);
}

/// <summary>
/// An activity of type <see cref="ActivityTypeEnum.DoWhileOrUntil"/>.
/// </summary>
public interface IActivityDoWhileOrUntil<TActivityReturns> : IExecutableActivity<TActivityReturns>
{
    /// <summary>
    /// Continue the loop until <paramref name="conditionMethodAsync"/> return true.
    /// </summary>
    IActivityDoWhileOrUntil<TActivityReturns> Until(ActivityConditionMethodAsync conditionMethodAsync);

    /// <summary>
    /// Continue the loop until <paramref name="conditionMethod"/> return true.
    /// </summary>
    IActivityDoWhileOrUntil<TActivityReturns> Until(ActivityConditionMethod conditionMethod);

    /// <summary>
    /// Continue the loop until <paramref name="condition"/> is true.
    /// </summary>
    IActivityDoWhileOrUntil<TActivityReturns> Until(bool condition);

    /// <summary>
    /// Continue the loop until <paramref name="conditionMethodAsync"/> return true.
    /// </summary>
    IActivityDoWhileOrUntil<TActivityReturns> While(ActivityConditionMethodAsync conditionMethodAsync);

    /// <summary>
    /// Continue the loop until <paramref name="conditionMethod"/> return true.
    /// </summary>
    IActivityDoWhileOrUntil<TActivityReturns> While(ActivityConditionMethod conditionMethod);

    /// <summary>
    /// Continue the loop until <paramref name="condition"/> is true.
    /// </summary>
    IActivityDoWhileOrUntil<TActivityReturns> While(bool condition);
}