using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Support;

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces
{
    /// <summary>
    /// The implementation method for an activity.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TMethodReturnType">The type of the returned value from the method</typeparam>
    public delegate Task<TMethodReturnType> ActivityMethod<TMethodReturnType>(CancellationToken cancellationToken);

    /// <summary>
    /// The implementation method for an activity with no return value.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public delegate Task ActivityMethod(CancellationToken cancellationToken);

    /// <summary>
    /// Basic information about a workflow activity, this is the base information for all types of activities.
    /// </summary>
    public interface IActivity : IWorkflowLogger
    {

        /// <summary>
        /// The instance id of the workflow
        /// </summary>
        string WorkflowInstanceId { get; }

        /// <summary>
        /// The date and time when the workflow started
        /// </summary>
        DateTimeOffset WorkflowStartedAt { get; }

        /// <summary>
        /// The instance id of the activity
        /// </summary>
        string ActivityInstanceId { get; }

        /// <summary>
        /// The form id of the activity
        /// </summary>
        string ActivityFormId { get; }

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
        /// The <see cref="ActivityOptions"/> for this activity.
        /// </summary>
        ActivityOptions Options { get; }

        /// <summary>
        /// The fail urgency for this activity
        /// </summary>
        [Obsolete("Please use Options.FailUrgency. Compilation warning since 2021-11-19.")]
        ActivityFailUrgencyEnum FailUrgency { get; }

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