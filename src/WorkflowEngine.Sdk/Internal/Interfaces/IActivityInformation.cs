using System.Collections.Generic;
using JetBrains.Annotations;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Support;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;

internal interface IActivityInformation
{
    /// <summary>
    /// All available information about a workflow
    /// </summary>
    IWorkflowInformation Workflow { get; }

    /// <summary>
    /// If the current activity is a child activity, then this is the parent activity for this child, otherwise null.
    /// </summary>
    [CanBeNull]
    IInternalActivity Parent { get; }

    /// <summary>
    /// The previous activity
    /// </summary>
    [CanBeNull]
    IInternalActivity Previous { get; }

    /// <summary>
    /// The relative position in the workflow for this activity.
    /// </summary>
    int Position { get; }

    /// <summary>
    /// The identity of the activity form
    /// </summary>
    string FormId { get; }

    /// <summary>
    /// The title for this activity form
    /// </summary>
    public string FormTitle { get; }

    /// <summary>
    /// The type for this activity form
    /// </summary>
    public ActivityTypeEnum Type{ get; }

    /// <summary>
    /// The options for this activity
    /// </summary>
    ActivityOptions Options { get; }

    /// <summary>
    /// Log records to be stored to the log table
    /// </summary>
    public ICollection<LogCreate> Logs { get; }

    /// <summary>
    /// Define an activity parameter named <paramref name="name"/> with type <typeparamref name="T"/>.
    /// </summary>
    void DefineParameter<T>(string name);

    /// <summary>
    /// Set the parameter named <paramref name="name"/> to the value <paramref name="value"/>.
    /// </summary>
    void SetParameter<T>(string name, T value);

    /// <summary>
    /// Get the value of parameter <paramref name="parameterName"/> with type <typeparamref name="T"/>.
    /// </summary>
    T GetArgument<T>(string parameterName);
}