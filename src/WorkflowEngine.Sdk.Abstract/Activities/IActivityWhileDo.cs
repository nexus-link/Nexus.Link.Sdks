using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Abstract.Activities;

/// <summary>
/// An activity of type <see cref="ActivityTypeEnum.WhileDo"/>.
/// </summary>
public interface IActivityWhileDo : IExecutableActivity, ILoopActivity
{
    /// <summary>
    /// Do this until the while condition is true.
    /// </summary>
    IActivityWhileDo Do(ActivityMethodAsync<IActivityWhileDo> methodAsync);

    /// <summary>
    /// Do this until the while condition is true.
    /// </summary>
    IActivityWhileDo Do(ActivityMethod<IActivityWhileDo> method);
}

/// <summary>
/// An activity of type <see cref="ActivityTypeEnum.WhileDo"/>.
/// </summary>
public interface IActivityWhileDo<TActivityReturns> : IExecutableActivity<TActivityReturns>, ILoopActivity
{
    /// <summary>
    /// Do this until the while condition is true.
    /// </summary>
    IActivityWhileDo<TActivityReturns> Do(ActivityMethodAsync<IActivityWhileDo<TActivityReturns>, TActivityReturns> methodAsync);

    /// <summary>
    /// Do this until the while condition is true.
    /// </summary>
    IActivityWhileDo<TActivityReturns> Do(ActivityMethod<IActivityWhileDo<TActivityReturns>, TActivityReturns> method);

    /// <summary>
    /// Do this until the while condition is true.
    /// </summary>
    IActivityWhileDo<TActivityReturns> Do(TActivityReturns condition);
}