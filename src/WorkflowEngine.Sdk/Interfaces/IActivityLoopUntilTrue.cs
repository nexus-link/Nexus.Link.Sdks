using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces;

/// <summary>
/// An activity of type <see cref="ActivityTypeEnum.LoopUntilTrue"/>.
/// </summary>
[Obsolete("Please use DoUntil or WhileDo. Obsolete since 2022-05-02.")]
public interface IActivityLoopUntilTrueBase : IActivity, ILoopActivity
{
    /// <summary>
    /// Set this to true when you want to end the loop and false otherwise. Missing to set this will result in a runtime error.
    /// </summary>
    bool? EndLoop { get; set; }

    /// <summary>
    /// Get the loop argument with name <paramref name="name"/>.
    /// </summary>
    /// <typeparam name="T">The type of the argument.</typeparam>
    [Obsolete("Please use the GetContext() or TryGetContext() method. Obsolete since 2022-05-01.")]
    T GetLoopArgument<T>(string name);

    /// <summary>
    /// Set the loop argument with name <paramref name="name"/> to <paramref name="value"/>.
    /// </summary>
    [Obsolete("Please use the SetContext() method. Obsolete since 2022-05-01.")]
    void SetLoopArgument<T>(string name, T value);
}

/// <summary>
/// An activity of type <see cref="ActivityTypeEnum.LoopUntilTrue"/>.
/// </summary>
[Obsolete("Please use DoUntil or WhileDo. Obsolete since 2022-05-02.")]
public interface IActivityLoopUntilTrue : IActivityLoopUntilTrueBase, IExecutableActivity
{
    /// <summary>
    /// Execute the method <paramref name="methodAsync"/>.
    /// </summary>
    [Obsolete("Please use the ExecuteAsync() method without a method in concert with ActivityType with method. Obsolete since 2022-05-01.")]
    Task ExecuteAsync(ActivityMethodAsync<IActivityLoopUntilTrue> methodAsync, CancellationToken cancellationToken = default);
}

/// <summary>
/// An activity of type <see cref="ActivityTypeEnum.LoopUntilTrue"/>.
/// </summary>
[Obsolete("Please use DoUntil or WhileDo. Obsolete since 2022-05-02.")]
public interface IActivityLoopUntilTrue<TActivityReturns> : IActivityLoopUntilTrueBase, IExecutableActivity<TActivityReturns>
{
    /// <summary>
    /// Execute the method <paramref name="methodAsync"/>.
    /// </summary>
    [Obsolete("Please use the ExecuteAsync() method without a method in concert with ActivityType with method. Obsolete since 2022-05-01.")]
    Task<TActivityReturns> ExecuteAsync(ActivityMethodAsync<IActivityLoopUntilTrue<TActivityReturns>, TActivityReturns> methodAsync, CancellationToken cancellationToken = default);
}