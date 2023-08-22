using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces;

/// <summary>
/// An activity of type <see cref="ActivityTypeEnum.If"/>.
/// </summary>
public interface IActivityIf : IActivity
{
    /// <summary>
    /// The method that decides if we should execute the then-method or the else-method.
    /// </summary>
    ActivityConditionMethodAsync<IActivityIf> ConditionMethodAsync { get; }

    /// <summary>
    /// Declare that <paramref name="methodAsync"/> should be executed if <see cref="ConditionMethodAsync"/> returns true.
    /// </summary>
    IActivityIfElse Then(ActivityMethodAsync<IActivityIf> methodAsync);

    /// <summary>
    /// Declare that <paramref name="method"/> should be executed if <see cref="ConditionMethodAsync"/> returns true.
    /// </summary>
    IActivityIfElse Then(ActivityMethod<IActivityIf> method);
}

/// <summary>
/// An activity of type <see cref="ActivityTypeEnum.If"/>.
/// </summary>
public interface IActivityIfElse : IExecutableActivity
{
    /// <summary>
    /// Declare that <paramref name="methodAsync"/> should be executed if <see cref="IActivityIf.ConditionMethodAsync"/> returns false.
    /// </summary>
    IExecutableActivity Else(ActivityMethodAsync<IActivityIf> methodAsync);

    /// <summary>
    /// Declare that <paramref name="method"/> should be executed if <see cref="IActivityIf.ConditionMethodAsync"/> returns false.
    /// </summary>
    IExecutableActivity Else(ActivityMethod<IActivityIf> method);
}

/// <summary>
/// An activity of type <see cref="ActivityTypeEnum.If"/>.
/// </summary>
public interface IActivityIf<TActivityReturns> : IActivity
{
    /// <summary>
    /// The method that decides if we should execute the then-method or the else-method.
    /// </summary>
    ActivityConditionMethodAsync<IActivityIf<TActivityReturns>> ConditionMethodAsync { get; }

    /// <summary>
    /// Declare that <paramref name="methodAsync"/> should be executed if <see cref="ConditionMethodAsync"/> returns true.
    /// </summary>
    IActivityIfElse<TActivityReturns> Then(ActivityMethodAsync<IActivityIf<TActivityReturns>, TActivityReturns> methodAsync);

    /// <summary>
    /// Declare that <paramref name="method"/> should be executed if <see cref="ConditionMethodAsync"/> returns true.
    /// </summary>
    IActivityIfElse<TActivityReturns> Then(ActivityMethod<IActivityIf<TActivityReturns>, TActivityReturns> method);

    /// <summary>
    /// Declare that <paramref name="value"/> should be returned if <see cref="ConditionMethodAsync"/> returns true.
    /// </summary>
    IActivityIfElse<TActivityReturns> Then(TActivityReturns value);
}

/// <summary>
/// An activity of type <see cref="ActivityTypeEnum.If"/>.
/// </summary>
public interface IActivityIfElse<TActivityReturns> :
    IExecutableActivity<TActivityReturns>
{
    /// <summary>
    /// Declare that <paramref name="methodAsync"/> should be executed if <see cref="IActivityIf{TActivityReturns}.ConditionMethodAsync"/> returns false.
    /// </summary>
    IExecutableActivity<TActivityReturns> Else(ActivityMethodAsync<IActivityIf<TActivityReturns>, TActivityReturns> methodAsync);

    /// <summary>
    /// Declare that <paramref name="method"/> should be executed if <see cref="IActivityIf{TActivityReturns}.ConditionMethodAsync"/> returns false.
    /// </summary>
    IExecutableActivity<TActivityReturns> Else(ActivityMethod<IActivityIf<TActivityReturns>, TActivityReturns> method);

    /// <summary>
    /// Declare that <paramref name="value"/> should be returned if <see cref="IActivityIf{TActivityReturns}.ConditionMethodAsync"/> returns false.
    /// </summary>
    IExecutableActivity<TActivityReturns> Else(TActivityReturns value);
}