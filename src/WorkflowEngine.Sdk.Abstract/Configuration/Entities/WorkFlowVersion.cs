using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Storage.Model;

namespace Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Entities;

/// <summary>
/// Information about a specific version of a <see cref="WorkflowForm"/>.
/// </summary>
public class WorkflowVersion : WorkflowVersionCreate, IUniquelyIdentifiable<string>, IOptimisticConcurrencyControlByETag
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
/// Information about a specific version of a <see cref="WorkflowForm"/>.
/// </summary>
public class WorkflowVersionCreate : IValidatable
{
    public string WorkflowFormId { get; set; }
    public int MajorVersion { get; set; }
    public int MinorVersion { get; set; }
    public bool DynamicCreate { get; set; }

    /// <inheritdoc />
    public virtual void Validate(string errorLocation, string propertyPath = "")
    {
        FulcrumValidate.IsNotNullOrWhiteSpace(WorkflowFormId, nameof(WorkflowFormId), errorLocation);
        FulcrumValidate.IsGreaterThanOrEqualTo(0, MajorVersion, nameof(MajorVersion), errorLocation);
        FulcrumValidate.IsGreaterThanOrEqualTo(0, MinorVersion, nameof(MinorVersion), errorLocation);
    }

    /// <inheritdoc />
    public override string ToString() => $"{MajorVersion}.{MinorVersion}";
}