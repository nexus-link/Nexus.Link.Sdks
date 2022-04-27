using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces;

/// <summary>
/// The implementation method for an action activity with no return value.
/// </summary>
/// <param name="activity">The current <see cref="IActivityLoopUntilTrue"/>.</param>
/// <param name="cancellationToken"></param>
/// <returns></returns>
public delegate Task ActivityLoopUntilMethodAsync(IActivityLoopUntilTrue activity, CancellationToken cancellationToken);

/// <summary>
/// The implementation method for an action activity with a return value.
/// </summary>
/// <param name="activity">The current <see cref="IActivityLoopUntilTrue{TMethodReturns}"/>.</param>
/// <param name="cancellationToken"></param>
/// <typeparam name="TMethodReturns">The type of the returned value from the method</typeparam>
public delegate Task<TMethodReturns> ActivityLoopUntilMethodAsync<TMethodReturns>(IActivityLoopUntilTrue<TMethodReturns> activity, CancellationToken cancellationToken);

/// <summary>
/// An activity of type <see cref="ActivityTypeEnum.LoopUntilTrue"/>.
/// </summary>
public interface IActivityLoopUntilTrueBase : IActivity
{
    /// <summary>
    /// Set this to true when you want to end the loop and false otherwise. Missing to set this will result in a runtime error.
    /// </summary>
    bool? EndLoop { get; set; }

    /// <summary>
    /// Get the loop argument with name <paramref name="name"/>.
    /// </summary>
    /// <typeparam name="T">The type of the argument.</typeparam>
    T GetLoopArgument<T>(string name);

    /// <summary>
    /// Set the loop argument with name <paramref name="name"/> to <paramref name="value"/>.
    /// </summary>
    void SetLoopArgument<T>(string name, T value);
}

/// <summary>
/// An activity of type <see cref="ActivityTypeEnum.LoopUntilTrue"/>.
/// </summary>
public interface IActivityLoopUntilTrue : IActivityLoopUntilTrueBase
{
    /// <summary>
    /// Execute the method <paramref name="methodAsync"/>.
    /// </summary>
    Task ExecuteAsync(ActivityLoopUntilMethodAsync methodAsync, CancellationToken cancellationToken = default);
}

/// <summary>
/// An activity of type <see cref="ActivityTypeEnum.LoopUntilTrue"/>.
/// </summary>
public interface IActivityLoopUntilTrue<TActivityReturns> : IActivityLoopUntilTrueBase
{
    /// <summary>
    /// Execute the method <paramref name="methodAsync"/>.
    /// </summary>
    Task<TActivityReturns> ExecuteAsync(ActivityLoopUntilMethodAsync<TActivityReturns> methodAsync, CancellationToken cancellationToken = default);
}