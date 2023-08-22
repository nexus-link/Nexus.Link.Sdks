using System.Collections.Generic;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;

public class ActivitySummary
{
    public ActivityForm Form { get; set; }
    public ActivityVersion Version { get; set; }
    public ActivityInstance Instance { get; set; }

    /// <summary>
    /// Sorted child activities in position order
    /// </summary>
    public IReadOnlyList<ActivitySummary> Children { get; set; }

    /// <inheritdoc />
    public override string ToString() => $"{Form} {Version} {Instance}";
}