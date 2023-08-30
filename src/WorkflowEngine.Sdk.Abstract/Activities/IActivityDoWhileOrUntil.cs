using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Abstract.Activities;

/// <summary>
/// An activity of type <see cref="ActivityTypeEnum.DoWhileOrUntil"/>.
/// </summary>
public interface IActivityDoWhileOrUntil : ILoopActivity, IActivity
{
    /// <summary>
    /// Continue the loop until <paramref name="conditionMethodAsync"/> return true.
    /// </summary>
    IExecutableActivity Until(ActivityConditionMethodAsync<IActivityDoWhileOrUntil> conditionMethodAsync);

    /// <summary>
    /// Continue the loop until <paramref name="conditionMethod"/> return true.
    /// </summary>
    IExecutableActivity Until(ActivityConditionMethod<IActivityDoWhileOrUntil> conditionMethod);

    /// <summary>
    /// Continue the loop until <paramref name="condition"/> is true.
    /// </summary>
    IExecutableActivity Until(bool condition);

    /// <summary>
    /// Continue the loop until <paramref name="conditionMethodAsync"/> return true.
    /// </summary>
    IExecutableActivity While(ActivityConditionMethodAsync<IActivityDoWhileOrUntil> conditionMethodAsync);

    /// <summary>
    /// Continue the loop until <paramref name="conditionMethod"/> return true.
    /// </summary>
    IExecutableActivity While(ActivityConditionMethod<IActivityDoWhileOrUntil> conditionMethod);

    /// <summary>
    /// Continue the loop until <paramref name="condition"/> is true.
    /// </summary>
    IExecutableActivity While(bool condition);
}

/// <summary>
/// An activity of type <see cref="ActivityTypeEnum.DoWhileOrUntil"/>.
/// </summary>
public interface IActivityDoWhileOrUntil<TActivityReturns> : IExecutableActivity<TActivityReturns>, ILoopActivity
{
    /// <summary>
    /// Continue the loop until <paramref name="conditionMethodAsync"/> return true.
    /// </summary>
    IExecutableActivity<TActivityReturns> Until(ActivityConditionMethodAsync<IActivityDoWhileOrUntil<TActivityReturns>> conditionMethodAsync);

    /// <summary>
    /// Continue the loop until <paramref name="conditionMethod"/> return true.
    /// </summary>
    IExecutableActivity<TActivityReturns> Until(ActivityConditionMethod<IActivityDoWhileOrUntil<TActivityReturns>> conditionMethod);

    /// <summary>
    /// Continue the loop until <paramref name="condition"/> is true.
    /// </summary>
    IExecutableActivity<TActivityReturns> Until(bool condition);

    /// <summary>
    /// Continue the loop until <paramref name="conditionMethodAsync"/> return true.
    /// </summary>
    IExecutableActivity<TActivityReturns> While(ActivityConditionMethodAsync<IActivityDoWhileOrUntil<TActivityReturns>> conditionMethodAsync);

    /// <summary>
    /// Continue the loop until <paramref name="conditionMethod"/> return true.
    /// </summary>
    IExecutableActivity<TActivityReturns> While(ActivityConditionMethod<IActivityDoWhileOrUntil<TActivityReturns>> conditionMethod);

    /// <summary>
    /// Continue the loop until <paramref name="condition"/> is true.
    /// </summary>
    IExecutableActivity<TActivityReturns> While(bool condition);
}