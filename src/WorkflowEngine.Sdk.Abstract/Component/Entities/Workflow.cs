using System;
using System.Collections.Generic;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Abstract.Component.Entities;

public class Workflow
{
    /// <summary>
    /// Workflow (instance) id
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Workflow version id
    /// </summary>
    public string WorkflowVersionId { get; set; }

    /// <summary>
    /// Workflow form id
    /// </summary>
    public string WorkflowFormId { get; set; }

    /// <summary>
    /// Title of the workflow, including information from Form, Version and Instance
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Timestamp when the workflow (instance) started
    /// </summary>
    public DateTimeOffset StartedAt { get; set; }

    /// <summary>
    /// If not null, timestamp when the workflow (instance) finished
    /// </summary>
    public DateTimeOffset? FinishedAt { get; set; }

    /// <summary>
    /// If not null, timestamp when the workflow (instance) was cancelled (probably by an administrator)
    /// </summary>
    public DateTimeOffset? CancelledAt { get; set; }

    /// <summary>
    /// State of the workflow instance
    /// </summary>
    public WorkflowStateEnum State { get; set; }

    /// <summary>
    /// Top level activities (instances) of the workflow (instance)
    /// </summary>
    public List<Activity> Activities { get; set; }
}