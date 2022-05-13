using System;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Support;

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces;

/// <summary>
/// Properties that can be extracted from <see cref="IActivityInformation"/>
/// </summary>
public interface IActivityBase
{
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
    /// If the activity is part of a loop, this is the iteration count for that loop
    /// </summary>
    ///
    [Obsolete($"Please use {nameof(ILoopActivity.LoopIteration)} or {nameof(IActivityParallel.JobNumber)}.", false)]
    int? Iteration { get; }

    /// <summary>
    /// The date and time when the activity started
    /// </summary>
    DateTimeOffset ActivityStartedAt { get; }

    /// <summary>
    /// A string representation that is good for logging purposes.
    /// </summary>
    /// <returns></returns>
    string ToLogString();

    /// <summary>
    /// The date and time when the workflow started
    /// </summary>
    DateTimeOffset WorkflowStartedAt { get; }

    /// <summary>
    /// The instance id of the workflow
    /// </summary>
    string WorkflowInstanceId { get; }

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
    /// Set an activity context key-value.
    /// </summary>
    /// <typeparam name="T">The type of the data in the parameter.</typeparam>
    /// <param name="key">The name of the part of the context that we want to access.</param>
    /// <param name="value">The value of the parameter</param>
    /// <remarks>
    /// The activity context is made available for arbitrary use by the implementor. It is
    /// saved in the database between runs and reset to empty after the activity has been completed.
    /// </remarks>
    void SetContext<T>(string key, T value);

    /// <summary>
    /// Remove an activity context key-value.
    /// </summary>
    /// <typeparam name="T">The type of the data in the parameter.</typeparam>
    /// <param name="key">The name of the part of the context that we want to access.</param>
    /// <remarks>
    /// The activity context is made available for arbitrary use by the implementor. It is
    /// saved in the database between runs and reset to empty after the activity has been completed.
    /// </remarks>
    void RemoveContext(string key);

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