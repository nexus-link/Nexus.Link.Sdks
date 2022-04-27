using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces;

/// <summary>
/// The implementation method for an if activity with no return value.
/// </summary>
/// <param name="activity">The current <see cref="IActivityIf"/>.</param>
/// <param name="cancellationToken"></param>
/// <returns></returns>
public delegate Task ActivityIfMethodAsync(IActivityIf activity, CancellationToken cancellationToken);

/// <summary>
/// The implementation method for an if activity with a return value.
/// </summary>
/// <param name="activity">The current <see cref="IActivityIf{TMethodReturns}"/>.</param>
/// <param name="cancellationToken"></param>
/// <typeparam name="TMethodReturns">The type of the returned value from the method</typeparam>
public delegate Task<TMethodReturns> ActivityIfMethodAsync<TMethodReturns>(IActivityIf<TMethodReturns> activity, CancellationToken cancellationToken);

/// <summary>
/// The condition method for the if activity. If it returns true, the then-method is called, otherwise the else-method is called.
/// </summary>
/// <param name="activity">The current <see cref="IActivityIf"/>.</param>
/// <param name="cancellationToken"></param>
public delegate Task<bool> ActivityIfConditionMethodAsync(IActivity activity, CancellationToken cancellationToken);

/// <summary>
/// An activity of type <see cref="ActivityTypeEnum.If"/>.
/// </summary>
public interface IActivityIf : IActivity
{
    /// <summary>
    /// The method that decides if we should execute the then-method or the else-method.
    /// </summary>
    ActivityIfConditionMethodAsync ConditionMethodAsync { get; }

    /// <summary>
    /// Declare that <paramref name="methodAsync"/> should be executed if <see cref="ConditionMethodAsync"/> returns true.
    /// </summary>
    IActivityIf Then(ActivityIfMethodAsync methodAsync);

    /// <summary>
    /// Declare that <paramref name="methodAsync"/> should be executed if <see cref="ConditionMethodAsync"/> returns false.
    /// </summary>
    IActivityIf Else(ActivityIfMethodAsync methodAsync);

    /// <summary>
    /// Execute <see cref="ConditionMethodAsync"/>. If it returns true, then call the method from <see cref="Then"/>
    /// else call the method from <see cref="Else"/>.
    /// </summary>
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}
/// <summary>
/// An activity of type <see cref="ActivityTypeEnum.If"/>.
/// </summary>
public interface IActivityIf<TActivityReturns> : IActivity
{
    /// <summary>
    /// The method that decides if we should execute the then-method or the else-method.
    /// </summary>
    ActivityIfConditionMethodAsync ConditionMethodAsync { get; }

    /// <summary>
    /// Declare that <paramref name="methodAsync"/> should be executed if <see cref="ConditionMethodAsync"/> returns true.
    /// </summary>
    IActivityIf<TActivityReturns> Then(ActivityIfMethodAsync<TActivityReturns> methodAsync);

    /// <summary>
    /// Declare that <paramref name="methodAsync"/> should be executed if <see cref="ConditionMethodAsync"/> returns false.
    /// </summary>
    IActivityIf<TActivityReturns> Else(ActivityIfMethodAsync<TActivityReturns> methodAsync);

    /// <summary>
    /// Execute <see cref="ConditionMethodAsync"/>. If it returns true, then call the method from <see cref="Then"/>
    /// else call the method from <see cref="Else"/>.
    /// </summary>
    Task<TActivityReturns> ExecuteAsync(CancellationToken cancellationToken = default);
}