using System;
using System.Collections.Generic;
using Nexus.Link.Libraries.Core.Misc.Models;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Abstract.Component.Entities;

public class Activity
{
    /// <summary>
    /// Activity (instance) id
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Title of the activity, including information from Form
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// The type of activity
    /// </summary>
    public ActivityTypeEnum Type { get; set; }

    /// <summary>
    /// The nested position of the activity
    /// </summary>
    public string Position { get; set; }

    /// <summary>
    /// Timestamp when the activity (instance) started
    /// </summary>
    public DateTimeOffset StartedAt { get; set; }

    /// <summary>
    /// If not null, timestamp when the activity (instance) finished
    /// </summary>
    public DateTimeOffset? FinishedAt { get; set; }

    /// <summary>
    /// State of the activity (instance)
    /// </summary>
    public ActivityStateEnum State { get; set; }

    /// <summary>
    /// Json representation of the result of the activity, if any
    /// </summary>
    public string ResultAsJson { get; set; }

    /// <summary>
    /// An error message that is readable by business people
    /// </summary>
    public string FriendlyErrorMessage { get; set; }

    /// <summary>
    /// An error message that can contain technical details
    /// </summary>
    public string TechnicalErrorMessage { get; set; }

    /// <summary>
    /// Reference to a workflow that this activity waits for
    /// </summary>
    public AnnotatedId<string> WaitingForWorkflow { get; set; }

    /// <summary>
    /// A title associated with an individual iteration (or parallel job).
    /// </summary>
    public string IterationTitle { get; set; }

    /// <summary>
    /// Sub activities (instances) of this activity (instance)
    /// </summary>
    public List<Activity> Children { get; set; }
}