using System;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
using Nexus.Link.Libraries.Core.Assert;
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