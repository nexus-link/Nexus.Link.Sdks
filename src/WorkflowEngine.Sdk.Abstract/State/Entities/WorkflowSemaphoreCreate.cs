using System;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.EntityAttributes;

namespace Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;

/// <summary>
/// Data type for requesting raising a WorkflowSemaphore
/// </summary>
public class WorkflowSemaphoreCreate : WorkflowSemaphoreHolder
{
    /// <summary>
    /// The maximum concurrent active users of the semaphore
    /// </summary>
    [Validation.GreaterThanOrEqualTo(1)]
    public int Limit { get; set; }

    /// <summary>
    /// After how long time should this semaphore expire once it has been raised?
    /// </summary>
    public TimeSpan ExpirationTime { get; set; }

    /// <inheritdoc />
    public override string ToString() => $"{ResourceIdentifier} for workflow {WorkflowFormId}, instance {WorkflowInstanceId}";
}

/// <summary>
/// Data type for requesting raising a WorkflowSemaphore
/// </summary>
public class WorkflowSemaphoreHolder : IValidatable
{
    /// <summary>
    /// The workflow instance that raised this semaphore
    /// </summary>
    [Validation.NotNullOrWhitespace]
    public string WorkflowInstanceId { get; set; }

    /// <summary>
    /// The parent activity instance for the activity that raised this semaphore, or null if no parent exists.
    /// </summary>
    public string ParentActivityInstanceId { get; set; }

    /// <summary>
    /// The parent iteration for the activity that raised this semaphore, or null if no parent exists.
    /// </summary>
    public int? ParentIteration { get; set; }

    /// <summary>
    /// The workflow that this semaphore (of type lock) is for, or null for global semaphores (of type throttle)
    /// </summary>
    public string WorkflowFormId { get; set; }

    /// <summary>
    /// This value is used to distinguish different semaphores for the same workflow
    /// </summary>
    [Validation.NotNullOrWhitespace]
    public string ResourceIdentifier { get; set; }

    /// <inheritdoc />
    public override string ToString() => $"{ResourceIdentifier} for workflow {WorkflowFormId}, instance {WorkflowInstanceId}";

    /// <inheritdoc />
    public void Validate(string errorLocation, string propertyPath = "")
    {
        // We rely on the ValidationAttributes
    }
}