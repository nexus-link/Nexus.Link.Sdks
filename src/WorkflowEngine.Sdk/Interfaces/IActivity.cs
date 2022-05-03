using System;
using System.Threading;
using System.Threading.Tasks;

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
    /// Basic information about a workflow activity, this is the base information for all types of activities.
    /// </summary>
    public interface IActivity : IActivityBase, IWorkflowLogger
    {

        /// <summary>
        /// The title of this activity
        /// </summary>
        /// 
        string ActivityTitle { get; }

        /// <summary>
        /// The date and time when the activity started
        /// </summary>
        DateTimeOffset ActivityStartedAt { get; }

        /// <summary>
        /// If the activity is part of a loop, this is the iteration count for that loop
        /// </summary>
        ///
        [Obsolete($"Please use {nameof(ILoopActivity.LoopIteration)} or {nameof(IParallelActivity.JobNumber)}.", false)]
        int? Iteration { get; }

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

        /// <summary>
        /// Set an activity context key-value.
        /// </summary>
        /// <typeparam name="T">The type of the data in the parameter.</typeparam>
        /// <param name="key">The name of the part of the context that we want to access.</param>
        /// <param name="value">The value of the parameter</param>
        /// <returns>The context value for this part of the context.</returns>
        /// <remarks>
        /// The activity context is made available for arbitrary use by the implementor. It is
        /// saved in the database between runs and reset to empty after the activity has been completed.
        /// </remarks>
        void SetContext<T>(string key, T value);

        /// <summary>
        /// Get an activity context key-value.
        /// </summary>
        /// <typeparam name="T">The type of the data in the parameter.</typeparam>
        /// <param name="key">The name of the part of the context that we want to access.</param>
        /// <returns>The context value for this part of the context.</returns>
        /// <remarks>
        /// The activity context is made available for arbitrary use by the implementor. It is
        /// saved in the database between runs and reset to empty after the activity has been completed.
        /// </remarks>
        T GetContext<T>(string key);

        /// <summary>
        /// Get an activity context key-value.
        /// </summary>
        /// <typeparam name="T">The type of the data in the parameter.</typeparam>
        /// <param name="key">The name of the part of the context that we want to access.</param>
        /// <param name="value">The context value for this part of the context. Will be default(T) if the method returns false.</param>
        /// <returns>True if the key was found. This also means that <paramref name="value"/> has been set.</returns>
        /// <remarks>
        /// The activity context is made available for arbitrary use by the implementor. It is
        /// saved in the database between runs and reset to empty after the activity has been completed.
        /// </remarks>
        bool TryGetContext<T>(string key, out T value);
    }
}