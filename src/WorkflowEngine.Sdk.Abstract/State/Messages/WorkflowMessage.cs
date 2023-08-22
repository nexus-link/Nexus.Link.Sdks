using System;
using Nexus.Link.Libraries.Core.EntityAttributes;
using Nexus.Link.Libraries.Core.MultiTenant.Model;

namespace Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Messages;

/// <summary>
/// Base class for messages that are put on the queue for workflows.
/// </summary>
public abstract class WorkflowMessage
{
    /// <summary>
    /// The type of message.
    ///
    /// Example is <see cref="WorkflowInstanceChangedV1"/>.
    /// </summary>
    public string Type => this.GetType().Name;

    /// <summary>
    /// The identity of the source system
    /// </summary>
    [Validation.NotNullOrWhitespace]
    public string SourceClientId { get; set; }

    /// <summary>
    /// The <see cref="Tenant"/> of the source system
    /// </summary>
    [Validation.NotNull]
    public Tenant Tenant { get; set; }

    /// <summary>
    /// The time when the message was created
    /// </summary>
    [Validation.NotNull]
    [Validation.NotDefault]
    public DateTimeOffset Timestamp { get; set; }
}