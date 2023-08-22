using System;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Storage.Model;

namespace Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Entities;

public class WorkflowForm : WorkflowFormCreate, IUniquelyIdentifiable<string>, IOptimisticConcurrencyControlByETag
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
/// Information about a workflow.
/// </summary>
/// <remarks>
/// <see cref="CapabilityName"/> and <see cref="Title"/> in combination must be unique.
/// </remarks>
public class WorkflowFormCreate : IValidatable
{
    public string CapabilityName {get; set; }
    /// <summary>
    /// The name of the work flow
    /// </summary>
    public string Title {get; set; }

    /// <inheritdoc />
    public virtual void Validate(string errorLocation, string propertyPath = "")
    {
        FulcrumValidate.IsNotNullOrWhiteSpace(CapabilityName, nameof(CapabilityName), errorLocation);
        FulcrumValidate.IsNotNullOrWhiteSpace(Title, nameof(Title), errorLocation);
    }

    /// <inheritdoc />
    public override string ToString() => $"{Title}";
}


/// <summary>
/// A <see cref="WorkflowForm"/> decorated with instances counts per state.
/// </summary>
public class WorkflowFormOverview : WorkflowForm
{
    /// <summary>
    /// Instances overview
    /// </summary>
    public WorkflowFormInstancesOverview Overview { get; set; }
    /// <summary>
    /// Version
    /// </summary>
    public string  Version { get; set; }
}

/// <summary>
/// Instances count per state
/// </summary>
public class WorkflowFormInstancesOverview
{
    /// <summary>
    /// The start date of the instance counts
    /// </summary>
    public DateTimeOffset InstancesFrom { get; set; }

    /// <summary>
    /// The enddate of the instance counts
    /// </summary>
    public DateTimeOffset InstancesTo { get; set; }

    /// <summary>
    /// The total number of instances in the time interval
    /// </summary>
    public int InstanceCount { get; set; }

    /// <summary>
    /// The number of instances in the time interval with state Executing
    /// </summary>
    public int ExecutingCount { get; set; }

    /// <summary>
    /// The number of instances in the time interval with state Waiting
    /// </summary>
    public int WaitingCount { get; set; }

    /// <summary>
    /// The number of instances in the time interval with state Success
    /// </summary>
    public int SucceededCount { get; set; }

    /// <summary>
    /// The number of instances in the time interval with error states
    /// </summary>
    public int ErrorCount { get; set; }
}