using System.Collections.Generic;
using Newtonsoft.Json;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Support;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Support;

internal class ActivityInformation : IActivityInformation
{
    private readonly ActivityDefinition _activityDefinition;

    private readonly MethodHandler _methodHandler;

    public ActivityInformation(IWorkflowInformation workflowInformation, int position, string formId, string title = null)
    {
        InternalContract.RequireNotNull(workflowInformation, nameof(workflowInformation));
        InternalContract.RequireGreaterThanOrEqualTo(1, position, nameof(position));
        InternalContract.RequireNotNullOrWhiteSpace(formId, nameof(formId));
        Workflow = workflowInformation;
        Position = position;
        FormId = formId;
        Parent = WorkflowStatic.Context.ParentActivity;
        Previous = WorkflowStatic.Context.LatestActivity;
        _activityDefinition = Workflow.GetActivityDefinition(formId, title);
        if (_activityDefinition == null)
        {
            throw new WorkflowFailedException(ActivityExceptionCategoryEnum.WorkflowImplementationError,
                $"The workflow container of {workflowInformation.ToLogString()} was expected to contain a definition for the activity with id {formId}",
                "The workflow was implemented in a bad way.");
        }
        Options.From(Workflow.DefaultActivityOptions);
        _methodHandler = new MethodHandler(FormTitle);
    }

    /// <inheritdoc />
    [JsonIgnore]
    public IInternalActivity Previous { get; }

    /// <inheritdoc />
    [JsonIgnore]
    public IInternalActivity Parent { get; }

    /// <inheritdoc />
    public string FormTitle => _activityDefinition.Title;

    private readonly object _lockObject = new();
    private string _instanceId;

    /// <inheritdoc />
    public string InstanceId
    {
        get
        {
            lock (_lockObject)
            {
                return _instanceId ??= Workflow.GetOrCreateActivityInstanceId(this);
            }
        }
    }

    public ActivityTypeEnum? Type
    {
        get => _activityDefinition.Type;
        set => _activityDefinition.Type = value;
    }

    /// <inheritdoc />
    public ActivityOptions Options { get; } = new();

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
    public string ToLogString() => $"{Type} {FormTitle} (form id: {FormId})";

    /// <inheritdoc />
    public IWorkflowInformation Workflow { get; }

    /// <inheritdoc />
    public int Position { get; }

    /// <inheritdoc />
    public string FormId { get; }

    /// <inheritdoc />
    public ICollection<LogCreate> Logs { get; } = new List<LogCreate>();
}