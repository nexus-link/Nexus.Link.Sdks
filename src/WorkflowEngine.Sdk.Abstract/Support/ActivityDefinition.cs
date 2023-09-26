using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Abstract.Support;

/// <summary>
/// The definition of an activity. This class is normally used in a WorkflowContainer.
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
    public ActivityTypeEnum? Type { get; set; }
}