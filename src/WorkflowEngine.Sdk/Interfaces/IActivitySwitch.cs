using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces;

/// <summary>
/// The implementation method for an if activity with no return value.
/// </summary>
/// <param name="activity">The current <see cref="IActivitySwitch"/>.</param>
/// <param name="cancellationToken"></param>
/// <returns></returns>
public delegate Task ActivitySwitchMethodAsync<TSwitchValue>(IActivitySwitch<TSwitchValue> activity, CancellationToken cancellationToken);

/// <summary>
/// The implementation method for an if activity with a return value.
/// </summary>
/// <param name="activity">The current <see cref="IActivitySwitch{TSwitchValue, TMethodReturns}"/>.</param>
/// <param name="cancellationToken"></param>
/// <typeparam name="TMethodReturns">The type of the returned value from the method</typeparam>
/// <typeparam name="TSwitchValue">The type for the value that we will switch over.</typeparam>
public delegate Task<TMethodReturns> ActivitySwitchMethodAsync<TMethodReturns, TSwitchValue>(IActivitySwitch<TMethodReturns, TSwitchValue> activity, CancellationToken cancellationToken);

/// <summary>
/// The condition method for the if activity. Switch it returns true, the then-method is called, otherwise the else-method is called.
/// </summary>
/// <param name="activity">The current <see cref="IActivitySwitch{TSwitchValue}"/>.</param>
/// <param name="cancellationToken"></param>
public delegate Task<TSwitchValue> ActivitySwitchValueMethodAsync<TSwitchValue>(IActivity activity, CancellationToken cancellationToken);

/// <summary>
/// The condition method for the if activity. Switch it returns true, the then-method is called, otherwise the else-method is called.
/// </summary>
/// <param name="activity">The current <see cref="IActivitySwitch{TSwitchValue}"/>.</param>
public delegate TSwitchValue ActivitySwitchValueMethod<out TSwitchValue>(IActivity activity);

/// <summary>
/// An activity of type <see cref="ActivityTypeEnum.Switch"/>.
/// </summary>
public interface IActivitySwitch<TSwitchValue> : IActivity
{
    /// <summary>
    /// The method that decides if we should execute the then-method or the else-method.
    /// </summary>
    ActivitySwitchValueMethodAsync<TSwitchValue> SwitchValueMethodAsync { get; }

    /// <summary>
    /// Declare that <paramref name="methodAsync"/> should be executed if <see cref="SwitchValueMethodAsync"/> returns a value that is Equal to <paramref name="caseValue"/>.
    /// </summary>
    IActivitySwitch<TSwitchValue> Case(TSwitchValue caseValue, ActivitySwitchMethodAsync<TSwitchValue> methodAsync);

    /// <summary>
    /// Declare that <paramref name="methodAsync"/> should be executed if <see cref="SwitchValueMethodAsync"/> returns a value that is not Equal to any of the case  values.
    /// </summary>
    IActivitySwitch<TSwitchValue> Default(ActivitySwitchMethodAsync<TSwitchValue> methodAsync);

    /// <summary>
    /// Execute <see cref="SwitchValueMethodAsync"/>, depending on the value, select one of the case methods or the default methods if none match.
    /// </summary>
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// An activity of type <see cref="ActivityTypeEnum.Switch"/>.
/// </summary>
public interface IActivitySwitch<TActivityReturns, TSwitchValue> : IActivity
{
    /// <summary>
    /// The method that decides if we should execute the then-method or the else-method.
    /// </summary>
    ActivitySwitchValueMethodAsync<TSwitchValue> SwitchValueMethodAsync { get; }

    /// <summary>
    /// Declare that <paramref name="methodAsync"/> should be executed if <see cref="SwitchValueMethodAsync"/> returns a value that is Equal to <paramref name="caseValue"/>.
    /// </summary>
    IActivitySwitch<TActivityReturns, TSwitchValue> Case(TSwitchValue caseValue, ActivitySwitchMethodAsync<TActivityReturns, TSwitchValue> methodAsync);

    /// <summary>
    /// Declare that <paramref name="value"/> should be returned if <see cref="SwitchValueMethodAsync"/> returns a value that is Equal to <paramref name="caseValue"/>.
    /// </summary>
    IActivitySwitch<TActivityReturns, TSwitchValue> Case(TSwitchValue caseValue, TActivityReturns value);

    /// <summary>
    /// Declare that <paramref name="methodAsync"/> should be executed if <see cref="SwitchValueMethodAsync"/> returns a value that is not Equal to any of the case  values.
    /// </summary>
    IActivitySwitch<TActivityReturns, TSwitchValue> Default(ActivitySwitchMethodAsync<TActivityReturns, TSwitchValue> methodAsync);

    /// <summary>
    /// Declare that <paramref name="value"/> should be returned if <see cref="SwitchValueMethodAsync"/> returns a value that is not Equal to any of the case  values.
    /// </summary>
    IActivitySwitch<TActivityReturns, TSwitchValue> Default(TActivityReturns value);

    /// <summary>
    /// Execute <see cref="SwitchValueMethodAsync"/>, depending on the value, select one of the case methods or the default methods if none match.
    /// </summary>
    Task<TActivityReturns> ExecuteAsync(CancellationToken cancellationToken = default);
}