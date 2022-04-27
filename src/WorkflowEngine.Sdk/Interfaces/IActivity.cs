using System;
using System.Threading;
using System.Threading.Tasks;

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces
{
    /// <summary>
    /// The implementation method for an activity with no return value.
    /// </summary>
    public delegate Task ActivityMethodAsync(CancellationToken cancellationToken);

    /// <summary>
    /// The implementation method for an activity with a return value.
    /// </summary>
    /// <typeparam name="TMethodReturns">The type of the returned value from the method</typeparam>
    public delegate Task<TMethodReturns> ActivityMethodAsync<TMethodReturns>(CancellationToken cancellationToken);

    /// <summary>
    /// A method that returns a default value for an activity.
    /// </summary>
    /// <typeparam name="TMethodReturns">The type of the returned value from the method</typeparam>
    /// <remarks>
    /// The default value is used if the <see cref="ActivityMethodAsync{TMethodReturns}"/> throws an exception that can be ignored.
    /// </remarks>
    public delegate Task<TMethodReturns> ActivityDefaultValueMethodAsync<TMethodReturns>(CancellationToken cancellationToken);

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