﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract.Entities;
using Nexus.Link.Libraries.Core.Threads;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Services;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Support;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;
using Activity = Nexus.Link.WorkflowEngine.Sdk.Internal.Logic.Activity;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;

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

    ICollection<LogCreate> Logs { get; }

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
    public IInternalActivity LatestActivity { get; }

    /// <summary>
    /// Get the definition for a specific activity
    /// </summary>
    /// <param name="activityFormId">The activity that we want the definition for.</param>
    /// <param name="title">The title for the activity</param>
    ActivityDefinition GetActivityDefinition(string activityFormId, string title);

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
    /// The total number of activity instances for this workflow instance
    /// </summary>
    int NumberOfActivityInstances { get; }

    /// <summary>
    /// Add the <paramref name="activity"/> to the list of known activities.
    /// </summary>
    void AddActivity(IInternalActivity activity);

    /// <summary>
    /// Get the instance id for the specified <paramref name="activityInformation"/>, either existing or a newly created.
    /// </summary>
    string GetOrCreateActivityInstanceId(IActivityInformation activityInformation);

    /// <summary>
    /// Load information about this workflow instance
    /// </summary>
    Task LoadAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Save the current workflow instance to storage.
    /// </summary>
    Task SaveToDbAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Save the current workflow instance to storage.
    /// </summary>
    Task SaveAsync(bool doAnInitialSaveToFallback, CancellationToken cancellationToken);

    /// <summary>
    /// Return true if the workflow instance already exists in storage.
    /// </summary>
    bool InstanceExists();

    /// <summary>
    /// Aggregate all status information from the workflow activities into one consolidated state for the workflow.
    /// </summary>
    void AggregateActivityInformation();

    /// <summary>
    /// Get an activity executor for <paramref name="activity"/>
    /// </summary>
    IActivityExecutor GetActivityExecutor(Activity activity);

    /// <summary>
    /// Get an activity executor for <paramref name="activity"/>
    /// </summary>
    ILogicExecutor GetLogicExecutor(IInternalActivity activity);

    bool TryGetActivity(string activityId, out Activity activity);

    bool TryGetActivity<TActivityReturns>(string activityId, out Activity<TActivityReturns> activity);
    TActivityReturns GetActivityResult<TActivityReturns>(string activityInstanceId);

    /// <summary>
    /// This token has shorter time than the token that the workflow engine is using. This means that when
    /// that time has expired, we will still have some time to finalize our work, such as saving the state
    /// to the database.
    /// </summary>
    CancellationToken ReducedTimeCancellationToken { get; }

    /// <summary>
    /// Whenever a workflow is called again, this timer is reset. It is used to know how long time
    /// the current run is taking.
    /// </summary>
    Stopwatch TimeSinceCurrentRunStarted { get; }

    WorkflowOptions WorkflowOptions { get; }
    
    /// <summary>
    /// This semaphore is used to make updating <see cref="HttpAsyncResponses"/> thread safe.
    /// </summary>
    NexusAsyncSemaphore ReadResponsesSemaphore { get; }

    /// <summary>
    /// All the responses awailable from Async Manager for the requests that this workflow instance is waiting for
    /// </summary>
    IDictionary<string, HttpResponse> HttpAsyncResponses { get; set; }

    /// <summary>
    /// A string representation of the workflow that is detailed enough for logging.
    /// </summary>
    /// <returns></returns>
    string ToLogString();
}