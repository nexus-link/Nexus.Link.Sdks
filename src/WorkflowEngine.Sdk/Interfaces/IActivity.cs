using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Support;

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces
{
    /// <summary>
    /// The implementation method for an activity with no return value.
    /// </summary>
    public delegate Task InternalActivityMethodAsync(CancellationToken cancellationToken);

    /// <summary>
    /// The implementation method for an activity with a return value.
    /// </summary>
    /// <typeparam name="TMethodReturns">The type of the returned value from the method</typeparam>
    public delegate Task<TMethodReturns> InternalActivityMethodAsync<TMethodReturns>(CancellationToken cancellationToken);

    /// <summary>
    /// The implementation method for an activity type with no return value.
    /// </summary>
    /// <param name="activity">The current <see cref="IActivityLock"/>.</param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TActivity">The activity type</typeparam>
    public delegate Task ActivityMethodAsync<in TActivity>(TActivity activity, CancellationToken cancellationToken);

    /// <summary>
    /// The implementation method for an activity type with no return value.
    /// </summary>
    /// <param name="activity">The current <see cref="IActivityLock"/>.</param>
    /// <typeparam name="TActivity">The activity type</typeparam>
    public delegate void ActivityMethod<in TActivity>(TActivity activity);

    /// <summary>
    /// The implementation method for an activity type with a return value.
    /// </summary>
    /// <param name="activity">The current <see cref="IActivityLock{TMethodReturns}"/>.</param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TActivity">The activity type</typeparam>
    /// <typeparam name="TMethodReturns">The type of the returned value from the method</typeparam>
    public delegate Task<TMethodReturns> ActivityMethodAsync<in TActivity, TMethodReturns>(TActivity activity, CancellationToken cancellationToken);

    /// <summary>
    /// The implementation method for an activity type with a return value.
    /// </summary>
    /// <param name="activity">The current <see cref="IActivityLock{TMethodReturns}"/>.</param>
    /// <typeparam name="TActivity">The activity type</typeparam>
    /// <typeparam name="TMethodReturns">The type of the returned value from the method</typeparam>
    public delegate TMethodReturns ActivityMethod<in TActivity, out TMethodReturns>(TActivity activity);

    /// <summary>
    /// A method that returns a default value for an activity.
    /// </summary>
    /// <typeparam name="TMethodReturns">The type of the returned value from the method</typeparam>
    /// <remarks>
    /// The default value is used if the <see cref="InternalActivityMethodAsync{TMethodReturns}"/> throws an exception that can be ignored.
    /// </remarks>
    public delegate Task<TMethodReturns> ActivityDefaultValueMethodAsync<TMethodReturns>(CancellationToken cancellationToken);

    /// <summary>
    /// The condition method for the if activity. If it returns true, the then-method is called, otherwise the else-method is called.
    /// </summary>
    /// <param name="activity">The current activity.</param>
    /// <param name="cancellationToken"></param>
    public delegate Task<bool> ActivityConditionMethodAsync<in TActivity>(TActivity activity, CancellationToken cancellationToken) where TActivity : IActivity;

    /// <summary>
    /// The condition method for the if activity. If it returns true, the then-method is called, otherwise the else-method is called.
    /// </summary>
    /// <param name="activity">The current activity.</param>
    public delegate bool ActivityConditionMethod<in TActivity>(TActivity activity) where TActivity : IActivity;

    /// <summary>
    /// The signature for a method that will be called whenever an activity fails.
    /// </summary>
    public delegate Task<bool> ActivityExceptionAlertMethodAsync(ActivityExceptionAlert alert, CancellationToken cancellationToken = default);

    /// <summary>
    /// A method to call when an activity throws an activity exception, <paramref name="exception"/>.
    /// </summary>
    /// <returns>True if the current activity thread should be abandoned and control should be given back to the parent.</returns>
    public delegate Task<bool> ActivityExceptionHandlerAsync(IActivity activity, ActivityFailedException exception, CancellationToken cancellationToken);

    /// <summary>
    /// Basic information about a workflow activity, this is the base information for all types of activities.
    /// </summary>
    public interface IActivity : IActivityBase, IWorkflowLogger
    {
        /// <summary>
        /// Access to the activity arguments
        /// </summary>
        /// <typeparam name="T">The type of the data in the parameter.</typeparam>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <returns>The argument value for this parameter.</returns>
        [Obsolete("Please use GetActivityArgument(). Compilation warning since 2021-11-18.")]
        T GetArgument<T>(string parameterName);

        /// <summary>
        /// Access to the activity arguments
        /// </summary>
        /// <typeparam name="T">The type of the data in the parameter.</typeparam>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <returns>The argument value for this parameter.</returns>
        T GetActivityArgument<T>(string parameterName);
    }
}