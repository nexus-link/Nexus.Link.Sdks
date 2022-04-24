using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowState.Abstract;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Services;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Support;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Support;

internal class WorkflowInformation : IWorkflowInformation
{
    private readonly IWorkflowImplementationBase _workflowImplementation;

    /// <summary>
    /// Caching of persistent data for this workflow instance
    /// </summary>
    private readonly WorkflowCache _workflowCache;

    public WorkflowInformation(IWorkflowImplementationBase workflowImplementation)
    {
        _workflowImplementation = workflowImplementation;
        _workflowCache = new WorkflowCache(this, workflowImplementation.WorkflowContainer.WorkflowCapabilities);
    }

    /// <inheritdoc />
    public string FormTitle => _workflowImplementation.WorkflowContainer.WorkflowFormTitle;

    /// <inheritdoc />
    public int MajorVersion => _workflowImplementation.MajorVersion;

    /// <inheritdoc />
    public int MinorVersion => _workflowImplementation.MinorVersion;

    private string _instanceTitle;

    /// <inheritdoc />
    public string InstanceTitle
    {
        get { return _instanceTitle ??= _workflowImplementation.GetInstanceTitle(); }
    }

    /// <inheritdoc />
    public DateTimeOffset StartedAt => _workflowCache.Instance.StartedAt;

    /// <inheritdoc />
    public ILogService LogService => _workflowImplementation.WorkflowContainer.WorkflowCapabilities.StateCapability.Log;

    /// <inheritdoc />
    public IWorkflowSemaphoreService SemaphoreService => _workflowImplementation.WorkflowContainer.WorkflowCapabilities.StateCapability.WorkflowSemaphore;

    /// <inheritdoc />
    public IWorkflowInstanceService WorkflowInstanceService => _workflowImplementation.WorkflowContainer.WorkflowCapabilities.StateCapability.WorkflowInstance;
    
    /// <inheritdoc />
    public ActivityOptions DefaultActivityOptions => _workflowImplementation.DefaultActivityOptions;
    

    /// <inheritdoc />
    public ActivityDefinition GetActivityDefinition(string activityFormId) =>
        _workflowImplementation.WorkflowContainer.GetActivityDefinition(activityFormId);

    /// <inheritdoc />
    public Activity LatestActivity { get; set; }

    /// <inheritdoc />
    public ICollection<string> ActivitiesToPurge { get; } = new List<string>();

    /// <inheritdoc />
    public WorkflowForm Form => _workflowCache.Form;

    /// <inheritdoc />
    public WorkflowVersion Version => _workflowCache.Version;

    /// <inheritdoc />
    public WorkflowInstance Instance => _workflowCache.Instance;

    /// <inheritdoc />
    public ActivityForm GetActivityForm(string activityFormId)
    {
        return _workflowCache.GetActivityForm(activityFormId);
    }

    /// <inheritdoc />
    public ActivityVersion GetActivityVersionByFormId(string activityFormId)
    {
        return _workflowCache.GetActivityVersionByFormId(activityFormId);
    }

    /// <inheritdoc />
    public ActivityInstance GetActivityInstance(string activityInstanceId)
    {
        return _workflowCache.GetActivityInstance(activityInstanceId);
    }

    /// <inheritdoc />
    public Activity GetCurrentParentActivity()
    {
        return _workflowCache.GetCurrentParentActivity();
    }

    /// <inheritdoc />
    public void AddActivity(Activity activity)
    {
        InternalContract.RequireNotNull(activity, nameof(activity));
        FulcrumAssert.IsNotNull(activity.Instance, CodeLocation.AsString());
        FulcrumAssert.IsNotNullOrWhiteSpace(activity.Instance.Id);
        _workflowCache.AddActivity(activity.Instance.Id, activity);
    }

    /// <inheritdoc />
    public Activity GetActivity(string activityInstanceId)
    {
        return _workflowCache.GetActivity(activityInstanceId);
    }

    /// <inheritdoc />
    public string GetOrCreateInstanceId(IActivityInformation activityInformation)
    {
        return _workflowCache.GetOrCreateInstanceId(activityInformation);
    }

    /// <inheritdoc />
    public Task LoadAsync(CancellationToken cancellationToken)
    {
        return _workflowCache.LoadAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task SaveAsync(CancellationToken cancellationToken)
    {
        return _workflowCache.SaveAsync(cancellationToken);
    }

    /// <inheritdoc />
    public bool InstanceExists()
    {
        return _workflowCache.InstanceExists();
    }

    /// <inheritdoc />
    public void AggregateActivityInformation()
    {
        _workflowCache.AggregateActivityInformation();
    }

    /// <inheritdoc />
    public string CapabilityName => _workflowImplementation.WorkflowContainer.WorkflowCapabilityName;

    /// <inheritdoc />
    public string FormId => _workflowImplementation.WorkflowContainer.WorkflowFormId;

    /// <inheritdoc />
    public string InstanceId { get; set; }
}

internal interface IWorkflowInformation
{

    /// <summary>
    /// The capability where this workflow is defined
    /// </summary>
    string CapabilityName { get; }

    /// <summary>
    /// The id of the workflow form
    /// </summary>
    string FormId { get; }

    /// <summary>
    /// The workflow form title
    /// </summary>
    string FormTitle { get; }

    /// <summary>
    /// The current <see cref="WorkflowForm"/>.
    /// </summary>
    WorkflowForm Form { get; }

    /// <summary>
    /// The major version for the workflow implementation
    /// </summary>
    int MajorVersion { get; }

    /// <summary>
    /// The minor version for the workflow implementation
    /// </summary>
    int MinorVersion { get; }

    /// <summary>
    /// The current <see cref="WorkflowVersion"/>.
    /// </summary>
    WorkflowVersion Version { get; }

    /// <summary>
    /// The current workflow instance id
    /// </summary>
    string InstanceId { get; set; }

    /// <summary>
    /// The current <see cref="WorkflowInstance"/>.
    /// </summary>
    WorkflowInstance Instance { get; }

    /// <summary>
    /// The title for the current workflow instance
    /// </summary>
    string InstanceTitle { get; }

    /// <summary>
    /// The date and time that we originally started this workflow instance
    /// </summary>
    DateTimeOffset StartedAt { get; }

    /// <summary>
    /// Service for saving logs to storage
    /// </summary>
    ILogService LogService { get; }

    /// <summary>
    /// Service for dealing with semaphores
    /// </summary>
    IWorkflowSemaphoreService SemaphoreService { get; }

    /// <summary>
    /// Service for dealing with workflow instances
    /// </summary>
    IWorkflowInstanceService WorkflowInstanceService { get; }

    /// <summary>
    /// The activity options for the current implementation
    /// </summary>
    ActivityOptions DefaultActivityOptions { get; }

    /// <summary>
    /// The latest activity that was activated
    /// </summary>
    public Activity LatestActivity { get; set; }

    /// <summary>
    /// If an activity should be purged, it should be added to this list.
    /// </summary>
    ICollection<string> ActivitiesToPurge { get; }

    /// <summary>
    /// Get the definition for a specific activity
    /// </summary>
    /// <param name="activityFormId">The activity that we want the definition for.</param>
    ActivityDefinition GetActivityDefinition(string activityFormId);

    /// <summary>
    /// Get the <see cref="ActivityForm"/> with id <paramref name="activityFormId"/>.
    /// </summary>
    ActivityForm GetActivityForm(string activityFormId);

    /// <summary>
    /// Get the <see cref="ActivityVersion"/> that has the form id <paramref name="activityFormId"/> for the current workflow version.
    /// </summary>
    /// <param name="activityFormId"></param>
    /// <returns></returns>
    ActivityVersion GetActivityVersionByFormId(string activityFormId);

    /// <summary>
    /// Get the <see cref="ActivityInstance"/> that has id <paramref name="activityInstanceId"/>.
    /// </summary>
    /// <param name="activityInstanceId"></param>
    /// <returns></returns>
    ActivityInstance GetActivityInstance(string activityInstanceId);

    /// <summary>
    /// Get the current parent activity, or null.
    /// </summary>
    Activity GetCurrentParentActivity();

    /// <summary>
    /// Add the <paramref name="activity"/> to the list of known activities. Use <see cref="GetActivity"/> to retrieve it later.
    /// </summary>
    void AddActivity(Activity activity);

    /// <summary>
    /// Get the <see cref="Activity"/> with instance id <paramref name="activityInstanceId"/>.
    /// </summary>
    Activity GetActivity(string activityInstanceId);

    /// <summary>
    /// Get the instance id for the specified <paramref name="activityInformation"/>, either existing or a newly created.
    /// </summary>
    string GetOrCreateInstanceId(IActivityInformation activityInformation);

    /// <summary>
    /// Load information about this workflow instance
    /// </summary>
    Task LoadAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Save the current workflow instance to storage.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SaveAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Return true if the workflow instance already exists in storage.
    /// </summary>
    bool InstanceExists();

    /// <summary>
    /// Aggregate all status information from the workflow activities into one consolidated state for the workflow.
    /// </summary>
    void AggregateActivityInformation();
}