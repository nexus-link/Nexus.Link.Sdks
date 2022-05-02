using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Support;

/// <summary>
/// The definition of an activity. This class is normally used in <see cref="WorkflowContainer"/>.
/// </summary>
/// <remarks>
/// This definition should be constant over versions.
/// </remarks>
public class ActivityDefinition
{
    /// <summary>
    /// The form id for the activity
    /// </summary>
    public string ActivityFormId { get; set; }

    /// <summary>
    /// The title of the activity
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// The type of the activity
    /// </summary>
    public ActivityTypeEnum Type { get; set; }
}