using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Support;

public class ActivityDefinition
{
    public string ActivityFormId { get; set; }
    public string Title { get; set; }
    public ActivityTypeEnum Type { get; set; }
}