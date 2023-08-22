using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Storage.Model;

namespace Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Entities;

public enum ActivityFailUrgencyEnum 
{
    /// <summary>
    /// The activity is hindering other activities to complete.
    /// </summary>
    /// <remarks>
    /// The default value for this enumeration
    /// </remarks>
    Stopping,
    /// <summary>
    /// If this activity fails, the entire workflow should be cancelled.
    /// </summary>
    CancelWorkflow,
    /// <summary>
    /// The activity does not hinder other activities, in fact the whole workflow can deliver a result even if this activity fails.
    /// It is important that the activity is completed anyway, so it should be dealt with eventually to complete the workflow entirely.
    /// </summary>
    HandleLater,
    /// <summary>
    /// The activity was of a "fire and forget" character, i.e. it doesn't matter if it failed, as long as we tried.
    /// </summary>
    Ignore
};

public class ActivityVersion : ActivityVersionCreate, IUniquelyIdentifiable<string>, IOptimisticConcurrencyControlByETag
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

public class ActivityVersionCreate : IValidatable
{
    public string WorkflowVersionId { get; set; }
    public string ActivityFormId { get; set; }
    public int Position { get; set; }
    public string ParentActivityVersionId { get; set; }
    public ActivityFailUrgencyEnum FailUrgency { get; set; }

    /// <inheritdoc />
    public virtual void Validate(string errorLocation, string propertyPath = "")
    {
        FulcrumValidate.IsNotNullOrWhiteSpace(WorkflowVersionId, nameof(WorkflowVersionId), errorLocation);
        FulcrumValidate.IsNotNullOrWhiteSpace(ActivityFormId, nameof(ActivityFormId), errorLocation);
        FulcrumValidate.IsGreaterThanOrEqualTo(1, Position, nameof(Position), errorLocation);
    }

    /// <inheritdoc />
    public override string ToString() => $"{Position}";
}