using System;
using System.Collections.Generic;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Context;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Support;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;

/// <summary>
/// Properties that can be extracted from <see cref="IActivityInformation"/>
/// </summary>
internal abstract class ActivityBase : IActivityBase, IInternalActivityBase
{
    protected ActivityBase(IActivityInformation activityInformation)
    {
        InternalContract.RequireNotNull(activityInformation, nameof(activityInformation));
        ActivityInformation = activityInformation;
        ActivityInstanceId = ActivityInformation.Workflow.GetOrCreateInstanceId(activityInformation);

        Form = ActivityInformation.Workflow.GetActivityForm(ActivityInformation.FormId);
        Version = ActivityInformation.Workflow.GetActivityVersionByFormId(ActivityInformation.FormId);
        Instance = ActivityInformation.Workflow.GetActivityInstance(ActivityInstanceId);
        var parentActivity = ActivityInformation.Parent;
        if (parentActivity != null)
        {
            NestedIterations.AddRange(parentActivity.NestedIterations);
            FulcrumAssert.IsNotNull(parentActivity.InternalIteration, CodeLocation.AsString());
            FulcrumAssert.IsGreaterThan(0, parentActivity.InternalIteration!.Value, CodeLocation.AsString());
            NestedIterations.Add(parentActivity.InternalIteration.Value);
            NestedPosition = $"{parentActivity.NestedPosition}.{ActivityInformation.Position}";
        }
        else
        {
            NestedPosition = $"{ActivityInformation.Position}";
        }
        var valueProvider = new AsyncLocalContextValueProvider();
        _internalIteration = new OneValueProvider<int?>(valueProvider, nameof(InternalIteration));
    }
    public string NestedPositionAndTitle => $"{NestedPosition} {ActivityInformation.FormTitle}";

    public List<int> NestedIterations { get; } = new();

    public string NestedPosition { get; }

    private readonly OneValueProvider<int?> _internalIteration;

    /// <inheritdoc />
    public int? InternalIteration
    {
        get => _internalIteration.GetValue();
        set => _internalIteration.SetValue(value);
    }

    public IActivityInformation ActivityInformation { get; }

    /// <inheritdoc />
    public string ActivityInstanceId { get; }

    /// <inheritdoc />
    [Obsolete("Please use Options.FailUrgency. Compilation warning since 2021-11-19.")]
    public ActivityFailUrgencyEnum FailUrgency => ActivityInformation.Options.FailUrgency;

    /// <inheritdoc />
    public string WorkflowInstanceId => ActivityInformation.Workflow.InstanceId;

    /// <inheritdoc />
    public DateTimeOffset WorkflowStartedAt => ActivityInformation.Workflow.StartedAt;

    /// <inheritdoc />
    public ActivityOptions Options => ActivityInformation.Options;
    
    public string ActivityFormId => ActivityInformation.FormId;

    /// <summary>
    /// The current <see cref="ActivityForm"/>
    /// </summary>
    protected ActivityForm Form { get; }
    /// <summary>
    /// The current <see cref="ActivityVersion"/>
    /// </summary>
    /// 
    public ActivityVersion Version { get; }

    /// <summary>
    /// The current <see cref="ActivityInstance"/>
    /// </summary>
    public ActivityInstance Instance { get; }

    /// <inheritdoc />
    public void MarkAsSuccess()
    {
        Instance.State = ActivityStateEnum.Success;
        Instance.FinishedAt = DateTimeOffset.UtcNow;
        Instance.ContextDictionary.Clear();
    }

    /// <inheritdoc />
#pragma warning disable CS0618
    public void MarkAsFailed(ActivityException exception)
#pragma warning restore CS0618
    {
        Instance.State = ActivityStateEnum.Failed;
        Instance.FinishedAt = DateTimeOffset.UtcNow;
        Instance.ContextDictionary.Clear();
        Instance.ExceptionCategory = exception.ExceptionCategory;
        Instance.ExceptionTechnicalMessage = exception.TechnicalMessage;
        Instance.ExceptionFriendlyMessage = exception.FriendlyMessage;
    }
}