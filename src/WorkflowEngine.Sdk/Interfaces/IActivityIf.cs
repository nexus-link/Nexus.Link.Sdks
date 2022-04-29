using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;

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
public interface IActivityIf : IActivity
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
    /// Declare that <paramref name="methodAsync"/> should be executed if <see cref="ConditionMethodAsync"/> returns false.
    /// </summary>
    IActivityIf Else(ActivityMethodAsync<IActivityIf> methodAsync);

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
    IActivityIf<TActivityReturns> Then(ActivityMethodAsync<IActivityIf<TActivityReturns>, TActivityReturns> methodAsync);

    /// <summary>
    /// Declare that <paramref name="value"/> should be returned if <see cref="ConditionMethodAsync"/> returns true.
    /// </summary>
    IActivityIf<TActivityReturns> Then(TActivityReturns value);

    /// <summary>
    /// Declare that <paramref name="methodAsync"/> should be executed if <see cref="ConditionMethodAsync"/> returns false.
    /// </summary>
    IActivityIf<TActivityReturns> Else(ActivityMethodAsync<IActivityIf<TActivityReturns>, TActivityReturns> methodAsync);

    /// <summary>
    /// Declare that <paramref name="value"/> should be returned if <see cref="ConditionMethodAsync"/> returns false.
    /// </summary>
    IActivityIf<TActivityReturns> Else(TActivityReturns value);

    /// <summary>
    /// Execute <see cref="ConditionMethodAsync"/>. If it returns true, then call the then-method"/>
    /// else call the else-method.
    /// </summary>
    Task<TActivityReturns> ExecuteAsync(CancellationToken cancellationToken = default);
}