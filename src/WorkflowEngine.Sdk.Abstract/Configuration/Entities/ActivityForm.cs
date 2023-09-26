using System;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Storage.Model;

namespace Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Entities;

/// <summary>
/// The accepted types of an activity, e.g. Action, Condition, etc.
/// </summary>
public enum ActivityTypeEnum
{
    /// <summary>The activity executes a task</summary>
    Action,
    /// <summary>
    /// The activity results in a value to be used as a condition
    /// </summary>
    [Obsolete("Please use If or Switch. Obsolete since 2022-04-27.")]
    Condition,
    /// <summary>
    /// One activity subset that  should only be executed if a specific condition is true and
    /// another subset that should only be executed if it is false.
    /// </summary>
    If,
    /// <summary>
    /// A number of activity subsets. Only one will be executed, depending on the value of an enumeration value.
    /// </summary>
    Switch,
    /// <summary>
    /// Loop over all sub activities until a condition is met
    /// </summary>
    [Obsolete("Please use WhileDo or DoUntil. Obsolete since 2022-05-02.")]
    LoopUntilTrue,
    /// <summary>
    /// Do the sub activities for each of the items in a list,
    /// all items are handled concurrently
    /// </summary>
    ForEachParallel,
    /// <summary>
    /// Do the sub activities for each of the items in a list,
    /// don't start with the next item until the previous item has completed.
    /// </summary>
    ForEachSequential,
    /// <summary>
    /// Raise or lower a semaphore, e.g. to limit resource access
    /// </summary>
    [Obsolete("Please use Throttle or Lock. Obsolete since 2002-03-14.")]
    Semaphore,
    /// <summary>
    /// Run a sub flow with a limited number of concurrent instances
    /// </summary>
    Throttle,
    /// <summary>
    /// Make sure that no other workflow instance is running this part of the workflow concurrently.
    /// </summary>
    Lock,
    /// <summary>
    /// Let the workflow sleep a while and then continue
    /// </summary>
    Sleep,
    /// <summary>
    /// Use this to run a number of activities in parallel.
    /// </summary>
    Parallel,
    /// <summary>
    /// Use this to loop until a condition has been met.
    /// </summary>
    DoWhileOrUntil,
    /// <summary>
    /// Use this to run a loop while a condition holds.
    /// </summary>
    WhileDo
}

public class ActivityForm : ActivityFormCreate, IUniquelyIdentifiable<string>, IOptimisticConcurrencyControlByETag
{
    public string Id { get; set; }
    public string Etag { get; set; }

    /// <inheritdoc />
    public override void Validate(string errorLocation, string propertyPath = "")
    {
        base.Validate(errorLocation, propertyPath);
        FulcrumValidate.IsNotNullOrWhiteSpace(Id, nameof(Id), errorLocation);
        FulcrumValidate.IsNotNullOrWhiteSpace(Etag, nameof(Etag), errorLocation);
    }
}

public class ActivityFormCreate : IValidatable
{
    public string WorkflowFormId { get; set; }
    public ActivityTypeEnum Type { get; set; }
    public string Title { get; set; }

    /// <inheritdoc />
    public virtual void Validate(string errorLocation, string propertyPath = "")
    {
        FulcrumValidate.IsNotNullOrWhiteSpace(WorkflowFormId, nameof(WorkflowFormId), errorLocation);
        FulcrumValidate.IsNotNullOrWhiteSpace(Title, nameof(Title), errorLocation);
    }

    /// <inheritdoc />
    public override string ToString() => $"{Type} {Title}";
}