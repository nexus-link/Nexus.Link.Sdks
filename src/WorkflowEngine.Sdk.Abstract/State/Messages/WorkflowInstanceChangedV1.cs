using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.EntityAttributes;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Messages;

/// <summary>
/// Fired when a <see cref="WorkflowInstance"/> is created or changed.
///
/// The properties that trigger a change can be (but not limited to)
/// - Title
/// - MajorVersion
/// - State
/// - Finished at
/// - Form title
/// - Tags
/// </summary>
public class WorkflowInstanceChangedV1 : WorkflowMessage, IValidatable
{
    /// <summary>
    /// The form for the instance
    /// </summary>
    [Validation.NotNull]
    public WorkflowForm Form { get; set; }

    /// <summary>
    /// The version of the instance
    /// </summary>
    [Validation.NotNull]
    public WorkflowVersion Version { get; set; }

    /// <summary>
    /// The instance that has changed
    /// </summary>
    [Validation.NotNull]
    public WorkflowInstance Instance { get; set; }

    /// <inheritdoc />
    public void Validate(string errorLocation, string propertyPath = "")
    {
        FulcrumValidate.IsNotNullOrWhiteSpace(Form.Id, $"{nameof(Form)}.{nameof(Form.Id)}", errorLocation);
        FulcrumValidate.IsNotNullOrWhiteSpace(Form.Title, $"{nameof(Form)}.{nameof(Form.Title)}", errorLocation);
        FulcrumValidate.IsNotNullOrWhiteSpace(Instance.Id, $"{nameof(Instance)}.{nameof(Instance.Id)}", errorLocation);
        FulcrumValidate.IsNotNullOrWhiteSpace(Instance.Title, $"{nameof(Instance)}.{nameof(Instance.Title)}", errorLocation);
        FulcrumValidate.IsNotDefaultValue(Instance.StartedAt, $"{nameof(Instance)}.{nameof(Instance.StartedAt)}", errorLocation);
    }
}