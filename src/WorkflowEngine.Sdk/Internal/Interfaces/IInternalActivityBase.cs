using System.Collections.Generic;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Exceptions;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;

internal interface IInternalActivityBase
{
    IActivityInformation ActivityInformation { get; }
    void MarkAsSuccess();
    void MarkAsSuccess<T>(T result);
#pragma warning disable CS0618
    void MarkAsFailed(ActivityException exception);
#pragma warning restore CS0618
    void MarkFailedForRetry();

    List<int> NestedIterations { get; }
    string NestedPosition { get; }

    /// <summary>
    /// If the activity is part of a loop, this is the iteration count for that loop
    /// </summary>
    ///
    int? InternalIteration { get; set; }

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
    void SetInternalContext<T>(string key, T value);

    /// <summary>
    /// Remove an activity context key-value.
    /// </summary>
    /// <param name="key">The name of the part of the context that we want to access.</param>
    /// <remarks>
    /// The activity context is made available for arbitrary use by the implementor. It is
    /// saved in the database between runs and reset to empty after the activity has been completed.
    /// </remarks>
    void RemoveInternalContext(string key);

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
    T GetInternalContext<T>(string key);

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
    bool TryGetInternalContext<T>(string key, out T value);
}