﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract.Entities;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Core.Threads;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Execution;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Services;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Support;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;
using Activity = Nexus.Link.WorkflowEngine.Sdk.Internal.Logic.Activity;

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
        TimeSinceCurrentRunStarted = new();
        TimeSinceCurrentRunStarted.Start();
        _workflowImplementation = workflowImplementation;
        _workflowCache = new WorkflowCache(this, workflowImplementation.WorkflowContainer.WorkflowCapabilities);
        WorkflowOptions = new WorkflowOptions().From(workflowImplementation.WorkflowContainer.WorkflowCapabilities
            .StateCapability.WorkflowInstance.DefaultWorkflowOptions);
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
    [JsonIgnore]
    public ICollection<LogCreate> Logs { get; } = new List<LogCreate>();

    /// <inheritdoc />
    public IWorkflowSemaphoreService SemaphoreService => _workflowImplementation.WorkflowContainer.WorkflowCapabilities.StateCapability.WorkflowSemaphore;

    /// <inheritdoc />
    public IWorkflowInstanceService WorkflowInstanceService => _workflowImplementation.WorkflowContainer.WorkflowCapabilities.StateCapability.WorkflowInstance;

    /// <inheritdoc />
    public ActivityOptions DefaultActivityOptions => _workflowImplementation.DefaultActivityOptions;


    /// <inheritdoc />
    public ActivityDefinition GetActivityDefinition(string activityFormId, string title) =>
        _workflowImplementation.WorkflowContainer.GetActivityDefinition(activityFormId) 
        ?? (title == null ? null : new ActivityDefinition
        {
            ActivityFormId = activityFormId,
            Title = title
        });

    /// <inheritdoc />
    [JsonIgnore]
    public IInternalActivity LatestActivity { get; set; }

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
    public void AddActivity(IInternalActivity activity)
    {
        InternalContract.RequireNotNull(activity, nameof(activity));
        FulcrumAssert.IsNotNull(activity.Instance, CodeLocation.AsString());
        FulcrumAssert.IsNotNullOrWhiteSpace(activity.Instance.Id);
        _workflowCache.AddActivity(activity.Instance.Id, activity);
    }

    /// <inheritdoc />
    public string GetOrCreateActivityInstanceId(IActivityInformation activityInformation)
    {
        return _workflowCache.GetOrCreateInstanceId(activityInformation);
    }

    /// <inheritdoc />
    public Task LoadAsync(CancellationToken cancellationToken)
    {
        return _workflowCache.LoadAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task SaveAsync(CancellationToken cancellationToken)
    {

        // Save the logs and possibly purge logs
        try
        {
            await SaveAndPurgeLogs();
        }
        catch (Exception)
        {
            if (FulcrumApplication.IsInDevelopment) throw;
            // Don't let logging problems get in our way
        }
        await _workflowCache.SaveAsync(true, cancellationToken);

        async Task SaveAndPurgeLogs()
        {
            var purge = DefaultActivityOptions.LogPurgeStrategy switch
            {
                LogPurgeStrategyEnum.AfterActivitySuccess => true,
                LogPurgeStrategyEnum.AfterWorkflowReturn => true,
                LogPurgeStrategyEnum.AfterWorkflowSuccess => Instance.State == WorkflowStateEnum.Success,
                LogPurgeStrategyEnum.AfterWorkflowComplete => Instance.IsComplete,
                _ => false
            };

            // Save the logs
            foreach (var logCreate in Logs)
            {
                if (purge && BelowOrEqualToThreshold(logCreate)) continue;
                await LogService.CreateAsync(logCreate, cancellationToken);
            }

            if (purge)
            {
                await LogService.DeleteWorkflowChildrenAsync(InstanceId,
                    DefaultActivityOptions.LogPurgeThreshold, cancellationToken);
            }
        }

        bool BelowOrEqualToThreshold(LogCreate logCreate)
        {
            return (int)logCreate.SeverityLevel <= (int)DefaultActivityOptions.LogPurgeThreshold;
        }
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
    public IActivityExecutor GetActivityExecutor(Activity activity)
    {
        return new ActivityExecutor(activity);
    }

    /// <inheritdoc />
    public ILogicExecutor GetLogicExecutor(IInternalActivity activity)
    {
        return new LogicExecutor(activity);
    }

    /// <inheritdoc />
    public bool TryGetActivity(string activityId, out Activity activity)
    {
        return _workflowCache.TryGetActivity(activityId, out activity);
    }

    /// <inheritdoc />
    public bool TryGetActivity<TActivityReturns>(string activityId, out Activity<TActivityReturns> activity)
    {
        return _workflowCache.TryGetActivity(activityId, out activity);
    }

    /// <inheritdoc />
    public TActivityReturns GetActivityResult<TActivityReturns>(string activityInstanceId)
    {
        // https://stackoverflow.com/questions/5780888/casting-interfaces-for-deserialization-in-json-net
        var instance = GetActivityInstance(activityInstanceId);
        FulcrumAssert.IsNotNull(instance, CodeLocation.AsString());
        FulcrumAssert.IsTrue(instance.HasCompleted, CodeLocation.AsString());
        if (instance.State == ActivityStateEnum.Success)
        {
            FulcrumAssert.IsNotNull(instance.ResultAsJson);
            try
            {
                var deserializedObject = JsonConvert.DeserializeObject<TActivityReturns>(instance.ResultAsJson);
                return deserializedObject;
            }
            catch (Exception e)
            {
                throw new FulcrumAssertionFailedException(
                    $"Could not deserialize activity {this} to type {typeof(TActivityReturns).Name}:{e}\r{instance.ResultAsJson}");
            }
        }
        FulcrumAssert.IsNotNull(instance.ExceptionCategory, CodeLocation.AsString());
        throw new ActivityFailedException(instance.ExceptionCategory!.Value, instance.ExceptionTechnicalMessage,
            instance.ExceptionFriendlyMessage);
    }

    /// <inheritdoc />
    public CancellationToken ReducedTimeCancellationToken => _workflowImplementation.ReducedTimeCancellationToken;

    /// <inheritdoc />
    public Stopwatch TimeSinceCurrentRunStarted { get; }

    public WorkflowOptions WorkflowOptions { get; }

    /// <inheritdoc />
    public NexusAsyncSemaphore ReadResponsesSemaphore { get; } = new(1);

    /// <inheritdoc />
    public IDictionary<string, HttpResponse> HttpAsyncResponses { get; set; }

    /// <inheritdoc />
    public string ToLogString()
    {
        string title;
        try
        {
            title = InstanceTitle;
        }
        catch (Exception)
        {
            title = FormTitle;
        }

        var id = !string.IsNullOrWhiteSpace(InstanceId) ? $"instance id: {InstanceId}" : $"form id: {FormId}";
        var state = Instance == null ? "" : Instance.State.ToString();
        return $"{title}{state} (id)";
    }

    /// <inheritdoc />
    public string CapabilityName => _workflowImplementation.WorkflowContainer.WorkflowCapabilityName;

    /// <inheritdoc />
    public string FormId => _workflowImplementation.WorkflowContainer.WorkflowFormId;

    /// <inheritdoc />
    public string InstanceId { get; set; }
}