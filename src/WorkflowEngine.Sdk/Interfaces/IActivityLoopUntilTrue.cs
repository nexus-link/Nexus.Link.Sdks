using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces;

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
    Task ExecuteAsync(ActivityMethodAsync<IActivityLoopUntilTrue> methodAsync, CancellationToken cancellationToken = default);
}

/// <summary>
/// An activity of type <see cref="ActivityTypeEnum.LoopUntilTrue"/>.
/// </summary>
public interface IActivityLoopUntilTrue<TActivityReturns> : IActivityLoopUntilTrueBase
{
    /// <summary>
    /// Execute the method <paramref name="methodAsync"/>.
    /// </summary>
    Task<TActivityReturns> ExecuteAsync(ActivityMethodAsync<IActivityLoopUntilTrue<TActivityReturns>, TActivityReturns> methodAsync, CancellationToken cancellationToken = default);
}