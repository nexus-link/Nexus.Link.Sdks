using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Newtonsoft.Json.Linq;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Json;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Support;
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

        // TODO: Currently these things have to be done in this order, because there is magic inside ActivityInstanceId. Should be changed.
        Instance = ActivityInformation.Workflow.GetActivityInstance(ActivityInstanceId);
        Version = ActivityInformation.Workflow.GetActivityVersionByFormId(ActivityInformation.FormId);
        Form = ActivityInformation.Workflow.GetActivityForm(ActivityInformation.FormId);
        var parentActivity = ActivityInformation.Parent;
        if (parentActivity != null)
        {
            try
            {
                NestedIterations.AddRange(parentActivity.NestedIterations);
                FulcrumAssert.IsNotNull(parentActivity.InternalIteration, CodeLocation.AsString());
                FulcrumAssert.IsGreaterThan(0, parentActivity.InternalIteration!.Value, CodeLocation.AsString());
                NestedIterations.Add(parentActivity.InternalIteration.Value);
                NestedPosition = $"{parentActivity.NestedPosition}[{parentActivity.InternalIteration.Value}].{ActivityInformation.Position}";

                Instance.IterationTitle = WorkflowStatic.Context.IterationTitle;                
            }
            catch (FulcrumAssertionFailedException e)
            {
                // Add some information to the exception
                throw new FulcrumAssertionFailedException(
                    $"The parent {parentActivity?.ToLogString()} of the current activity ({ToLogString()}) is not correct: {e.Message}");
            }
        }
        else
        {
            NestedPosition = $"{ActivityInformation.Position}";
        }

        Instance.AbsolutePosition = NestedPosition;
        InternalIteration = 0;
    }

    /// <inheritdoc />
    public string ActivityTitle
    {
        get
        {
            if (!NestedIterations.Any()) return NestedPositionAndTitle;
            var iterations = string.Join(",", NestedIterations);
            return $"{NestedPositionAndTitle} [{iterations}]";
        }
    }

    /// <inheritdoc />
    public DateTimeOffset ActivityStartedAt => Instance.StartedAt;

    /// <inheritdoc />
    [Obsolete($"Please use {nameof(ILoopActivity.LoopIteration)}.", true)]
    public int? Iteration => InternalIteration;

    /// <inheritdoc />
    public override string ToString() => ActivityTitle;

    /// <inheritdoc />
    public string ToLogString() => $"{ActivityTitle} {Instance?.State} (instance id: {ActivityInstanceId})";

    public IDictionary<string, JToken> ContextDictionary => Instance.ContextDictionary;

    public string NestedPositionAndTitle => $"{NestedPosition} {ActivityInformation.FormTitle}";

    public List<int> NestedIterations { get; } = new();

    public string NestedPosition { get; }

    private readonly AsyncLocal<int?> _internalIterationAsAsyncLocal = new AsyncLocal<int?>();

    /// <inheritdoc />
    public int? InternalIteration
    {
        get => _internalIterationAsAsyncLocal.Value;
        set => _internalIterationAsAsyncLocal.Value = value;
    }

    private static string InternalContextIdentifier = "A05CF82F-ADF2-4A0E-8715-78C560421689";

    private string GetInternalContextKey(string key) => $"{key}-{InternalContextIdentifier}";

    /// <inheritdoc />
    public void SetInternalContext<T>(string key, T value)
    {
        SetContext(GetInternalContextKey(key), value);
    }

    /// <inheritdoc />
    public void RemoveInternalContext(string key)
    {
        RemoveContext(GetInternalContextKey(key));
    }

    /// <inheritdoc />
    public T GetInternalContext<T>(string key)
    {
        return GetContext<T>(GetInternalContextKey(key));
    }

    /// <inheritdoc />
    public bool TryGetInternalContext<T>(string key, out T value)
    {
        return TryGetContext(GetInternalContextKey(key), out value);
    }

    public IActivityInformation ActivityInformation { get; protected set; }

    /// <inheritdoc />
    public string ActivityInstanceId => ActivityInformation.InstanceId;

    /// <inheritdoc />
    [Obsolete("Please use Options.FailUrgency. Compilation warning since 2021-11-19.")]
    public ActivityFailUrgencyEnum FailUrgency => ActivityInformation.Options.FailUrgency;

    /// <inheritdoc />
    public string WorkflowInstanceId => ActivityInformation.Workflow.InstanceId;

    /// <inheritdoc />
    public bool HasCompleted => Instance.HasCompleted;

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
    }

    /// <inheritdoc />
    public void MarkAsSuccess<T>(T result)
    {
        MarkAsSuccess();
        Instance.ResultAsJson = result.ToJsonString();
    }

    /// <inheritdoc />
    public void SetContext<T>(string key, T value)
    {
        InternalContract.RequireNotNullOrWhiteSpace(key, nameof(key));
        FulcrumAssert.IsNotNull(ContextDictionary, CodeLocation.AsString());
        ContextDictionary[key] = value == null ? JValue.CreateNull() : JToken.FromObject(value);
    }

    /// <inheritdoc />
    public void RemoveContext(string key)
    {
        InternalContract.RequireNotNullOrWhiteSpace(key, nameof(key));
        FulcrumAssert.IsNotNull(ContextDictionary, CodeLocation.AsString());
        if (!ContextDictionary.ContainsKey(key)) return;
        ContextDictionary.Remove(key);
    }

    /// <inheritdoc />
    public T GetContext<T>(string key)
    {
        InternalContract.RequireNotNullOrWhiteSpace(key, nameof(key));
        FulcrumAssert.IsNotNull(ContextDictionary, CodeLocation.AsString());
        if (!ContextDictionary.ContainsKey(key))
        {
            throw new FulcrumNotFoundException($"Could not find key {key} in context dictionary for activity {ActivityInstanceId}.");
        }
        var jToken = ContextDictionary[key];
        FulcrumAssert.IsNotNull(jToken, CodeLocation.AsString());
        return jToken.ToObject<T>();
    }

    /// <inheritdoc />
    public bool TryGetContext<T>(string key, out T value)
    {
        InternalContract.RequireNotNullOrWhiteSpace(key, nameof(key));
        value = default;
        if (!ContextDictionary.ContainsKey(key)) return false;
        value = GetContext<T>(key);
        return true;
    }

    /// <inheritdoc />
#pragma warning disable CS0618
    public void MarkAsFailed(ActivityException exception)
#pragma warning restore CS0618
    {
        Instance.State = ActivityStateEnum.Failed;
        Instance.FinishedAt = DateTimeOffset.UtcNow;
        Instance.ExceptionCategory = exception.ExceptionCategory;
        Instance.ExceptionTechnicalMessage = exception.TechnicalMessage;
        Instance.ExceptionFriendlyMessage = exception.FriendlyMessage;
    }

    /// <inheritdoc />
    public void MarkFailedForRetry()
    {
        FulcrumAssert.AreEqual(ActivityStateEnum.Failed, Instance.State, CodeLocation.AsString());
        Instance.State = ActivityStateEnum.Executing;
        Instance.FinishedAt = null;
        Instance.ExceptionCategory = null;
        Instance.ExceptionTechnicalMessage = null;
        Instance.ExceptionFriendlyMessage = null;
    }
}