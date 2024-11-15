using System;
using System.Collections.Generic;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;

/// <summary>
/// Information about an instance of a <see cref="WorkflowVersion"/>.
/// </summary>
public class WorkflowInstance : WorkflowInstanceCreate, IUniquelyIdentifiable<string>, IOptimisticConcurrencyControlByETag
{
    /// <inheritdoc />
    public string Id { get; set; }

    /// <inheritdoc />
    public string Etag { get; set; }

    /// <inheritdoc />
    public override void Validate(string errorLocation, string propertyPath = "")
    {
        base.Validate(errorLocation, propertyPath);
        FulcrumValidate.IsNotNullOrWhiteSpace(Id, nameof(Id), errorLocation);
        FulcrumValidate.IsNotNullOrWhiteSpace(Etag, nameof(Etag), errorLocation);
    }
}

/// <summary>
/// Information about a specific version of a <see cref="WorkflowInstance"/>.
/// </summary>
public class WorkflowInstanceCreate : IValidatable
{
    public string WorkflowVersionId { get; set; }

    public string Title { get; set; }

    public string InitialVersion { get; set; }

    public DateTimeOffset StartedAt { get; set; }

    public WorkflowStateEnum State { get; set; }

    public DateTimeOffset? FinishedAt { get; set; }
        
    public DateTimeOffset? CancelledAt { get; set; }

    public bool IsComplete { get; set; }

    public string ResultAsJson { get; set; }

    public string ExceptionTechnicalMessage { get; set; }

    public string ExceptionFriendlyMessage { get; set; }

    /// <summary>
    /// This value can be used instead of normal authentication to continue a postponed execution.
    /// Works in concert with Async Manager, which will send this value in a specific header.
    /// </summary>
    public string ReentryAuthentication { get; set; }

    /// <inheritdoc />
    public virtual void Validate(string errorLocation, string propertyPath = "")
    {
        FulcrumValidate.IsNotNullOrWhiteSpace(WorkflowVersionId, nameof(WorkflowVersionId), errorLocation);
        FulcrumValidate.IsNotNullOrWhiteSpace(Title, nameof(Title), errorLocation);
        FulcrumValidate.IsNotNullOrWhiteSpace(InitialVersion, nameof(InitialVersion), errorLocation);
        FulcrumValidate.IsLessThanOrEqualTo(DateTimeOffset.Now, StartedAt, nameof(StartedAt), errorLocation);
        if (FinishedAt != null)
        {
            FulcrumValidate.IsLessThanOrEqualTo(DateTimeOffset.Now, FinishedAt.Value, nameof(FinishedAt), errorLocation);
            FulcrumValidate.IsGreaterThanOrEqualTo(StartedAt, FinishedAt.Value, nameof(FinishedAt), errorLocation);
        }
    }

    /// <inheritdoc />
    public override string ToString() => $"{Title}";
}

/// <summary>
/// Used when searching/filtering for workflow instances.
/// </summary>
public class WorkflowInstanceSearchDetails : IValidatable
{
    /// <summary>
    /// Earliest that <see cref="WorkflowInstanceCreate.StartedAt"/> can be.
    /// Mandatory.
    /// </summary>
    public DateTimeOffset From { get; set; }
    
    /// <summary>
    /// If given, the tatest that <see cref="WorkflowInstanceCreate.StartedAt"/> can be.
    /// </summary>
    public DateTimeOffset? To { get; set; }
    
    /// <summary>
    /// If given, search only instances for a specific <see cref="WorkflowForm"/>
    /// </summary>
    public string FormId { get; set; }

    /// <summary>
    /// If given, search only instances with the specified states (with OR)
    /// </summary>
    public IList<WorkflowStateEnum> States { get; set; }

    /// <summary>
    /// If given, the title must contain the text (ignoring case)
    /// </summary>
    public string TitlePart { get; set; }

    /// <summary>
    /// Ordering of the returned instances
    /// </summary>
    public WorkflowInstanceSearchOrder Order { get; set; } = new();

    /// <inheritdoc />
    public void Validate(string errorLocation, string propertyPath = "")
    {
        FulcrumValidate.IsNotDefaultValue(From, nameof(From), errorLocation);
        if (To.HasValue) FulcrumValidate.IsTrue(To > From, errorLocation, $"{nameof(To)} must be later than {nameof(From)}");
        FulcrumValidate.IsNotNull(Order, nameof(Order), errorLocation);
    }
}

/// <summary>
/// Setups the ordering of an instance search
/// </summary>
public class WorkflowInstanceSearchOrder
{
    /// <summary>
    /// The first ORDER BY field
    /// </summary>
    public WorkflowSearchOrderByEnum PrimaryOrderBy { get; set; } = WorkflowSearchOrderByEnum.StartedAt;

    /// <summary>
    /// ASC or DESC for the first ORDER BY field
    /// </summary>
    public bool PrimaryAscendingOrder { get; set; }

    /// <summary>
    /// If, given, the second ORDER BY field
    /// </summary>
    public WorkflowSearchOrderByEnum? SecondaryOrderBy { get; set; }

    /// <summary>
    /// ASC or DESC for the second ORDER BY field
    /// </summary>
    public bool SecondaryAscendingOrder { get; set; }
}


/// <summary>
/// Represents fields that can ordered on
/// </summary>
public enum WorkflowSearchOrderByEnum
{
    /// <summary>
    /// <see cref="WorkflowInstanceCreate.StartedAt"/>
    /// </summary>
    /// <remarks>For optimization reasons, this is now ordering by RecordCreatedAt instead. It should always be within a second of StartedAt.</remarks>
    StartedAt,

    /// <summary>
    /// <see cref="WorkflowInstanceCreate.FinishedAt"/>
    /// </summary>
    FinishedAt,

    /// <summary>
    /// <see cref="WorkflowInstanceCreate.Title"/>
    /// </summary>
    Title,
    
    /// <summary>
    /// <see cref="WorkflowInstanceCreate.State"/>
    /// </summary>
    State
}

/// <summary>
/// The possile values for the workflow state
/// </summary>
public enum WorkflowStateEnum
{
    /// <summary>
    /// The activity has been started (default value)
    /// </summary>
    /// <remarks>
    /// The default value for this enumeration
    /// </remarks>
    Executing,
    /// <summary>
    /// We are asynchronously waiting for the activity to finish
    /// </summary>
    Waiting,
    /// <summary>
    /// There is at least one activity that that has a problem, but there are some parts of the workflow that still are running.
    /// </summary>
    Halting,
    /// <summary>
    /// There is at least one activity that that has a problem and no activities can run until the problem has been resolved.
    /// </summary>
    Halted,
    /// <summary>
    /// The activity has finished successfully
    /// </summary>
    Success,
    /// <summary>
    /// The activity has finished, but it failed. <see cref="ActivityFailUrgencyEnum"/> for the level of urgency to deal with this.
    /// </summary>
    Failed
};