using System;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.State;

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces
{
    /// <summary>
    /// Information about a workflow activity
    /// </summary>
    public interface IActivity : IWorkflowLogger
    {
        /// <summary>
        /// The instance id of the workflow
        /// </summary>
        string WorkflowInstanceId { get; }

        /// <summary>
        /// The instance id of the activity
        /// </summary>
        string ActivityInstanceId { get; }

        /// <summary>
        /// The title of this activity
        /// </summary>
        /// 
        string ActivityTitle { get; }

        /// <summary>
        /// If the activity is part of a loop, this is the iteration count for that loop
        /// </summary>
        /// 
        int? Iteration { get; }

        /// <summary>
        /// The fail urgency for this activity
        /// </summary>
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