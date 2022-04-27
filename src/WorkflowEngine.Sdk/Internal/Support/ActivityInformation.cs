using System.Collections.Generic;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Support;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Support;

internal class ActivityInformation : IActivityInformation
{
    private readonly ActivityDefinition _activityDefinition;

    private readonly MethodHandler _methodHandler;

    public ActivityInformation(IWorkflowInformation workflowInformation, int position, string formId)
    {
        Workflow = workflowInformation;
        Position = position;
        FormId = formId;
        Parent = Workflow.GetCurrentParentActivity();
        Previous = Workflow.LatestActivity;
        _activityDefinition = Workflow.GetActivityDefinition(formId);
        Options.From(Workflow.DefaultActivityOptions);
        _methodHandler = new MethodHandler(FormTitle);
    }

    /// <inheritdoc />
    public IInternalActivity Previous { get; }

    /// <inheritdoc />

    public string FormTitle => _activityDefinition.Title;

    public ActivityTypeEnum Type => _activityDefinition.Type;

    /// <inheritdoc />
    public ActivityOptions Options { get; } = new ();

    /// <inheritdoc />
    public void DefineParameter<T>(string name)
    {
        _methodHandler.DefineParameter<T>(name);
    }

    /// <inheritdoc />
    public void SetParameter<T>(string name, T value)
    {
        _methodHandler.SetParameter(name, value);
    }

    /// <inheritdoc />
    public T GetArgument<T>(string parameterName)
    {
        return _methodHandler.GetArgument<T>(parameterName);
    }

    /// <inheritdoc />
    public IWorkflowInformation Workflow { get; }

    /// <inheritdoc />
    public int Position { get; }

    /// <inheritdoc />
    public string FormId { get; }

    /// <inheritdoc />
    public IInternalActivity Parent { get; }

    /// <inheritdoc />
    public ICollection<LogCreate> Logs { get; } = new List<LogCreate>();
}