﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Activities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Support;
using Nexus.Link.WorkflowEngine.Sdk.Internal.ActivityTypes;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Extensions.State;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Support;
using Log = Nexus.Link.Libraries.Core.Logging.Log;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;

/// <inheritdoc cref="IInternalActivity" />
internal abstract class Activity : ActivityBase, IInternalActivity, IExecutableActivity, ISpawnedActivity
{
    protected Activity(IActivityInformation activityInformation)
        : base(activityInformation)
    {
        ActivityExecutor = ActivityInformation.Workflow.GetActivityExecutor(this);
        LogicExecutor = ActivityInformation.Workflow.GetLogicExecutor(this);
        ActivityInformation.Workflow.AddActivity(this);
        WorkflowStatic.Context.LatestActivity = this;
    }

    [JsonIgnore]
    protected IActivityExecutor ActivityExecutor { get; }

    [JsonIgnore]
    public ILogicExecutor LogicExecutor { get; }

    [Obsolete("Please use Options.AsyncRequestPriority. Compilation warning since 2021-11-19.")]
    [JsonIgnore]
    public double AsyncRequestPriority => Options.AsyncRequestPriority;

    [Obsolete("Please use Options.ExceptionAlertHandler. Compilation warning since 2021-11-19.")]
    [JsonIgnore]
    public ActivityExceptionAlertMethodAsync ExceptionAlertHandler => Options.ExceptionAlertHandler;

    /// <inheritdoc />
    public async Task LogAtLevelAsync(LogSeverityLevel severityLevel, string message, object data = null, CancellationToken cancellationToken = default)
    {
        Log.LogOnLevel(severityLevel, message);

        if (ActivityInformation.Workflow.LogService == null) return;
        try
        {
            if ((int)severityLevel < (int)Options.LogCreateThreshold) return;
            var jToken = WorkflowStatic.SafeConvertToJToken(data);
            var log = new LogCreate
            {
                WorkflowFormId = ActivityInformation.Workflow.FormId,
                WorkflowInstanceId = WorkflowInstanceId,
                ActivityFormId = Form?.Id,
                SeverityLevel = severityLevel,
                Message = message,
                Data = jToken,
                TimeStamp = DateTimeOffset.UtcNow,
            };
            ActivityInformation.Logs.Add(log);
        }
        catch (Exception)
        {
            if (FulcrumApplication.IsInDevelopment) throw;
            // Ignore logging problems when not in development mode.
        }

        await Task.CompletedTask;
    }

    /// <inheritdoc />
    [Obsolete("Please use GetActivityArgument(). Compilation warning since 2021-11-18.")]
    public TParameter GetArgument<TParameter>(string parameterName)
    {
        return GetActivityArgument<TParameter>(parameterName);
    }

    /// <inheritdoc />
    public T GetActivityArgument<T>(string parameterName)
    {
        return ActivityInformation.GetArgument<T>(parameterName);
    }

    /// <inheritdoc />
    public ActivityFailedException GetException()
    {
        if (Instance.State != ActivityStateEnum.Failed) return null;
        FulcrumAssert.IsNotNull(Instance.ExceptionCategory, CodeLocation.AsString());
        return new ActivityFailedException(Instance.ExceptionCategory!.Value, Instance.ExceptionTechnicalMessage,
            Instance.ExceptionFriendlyMessage);
    }

    public void PromoteOrPurgeLogs()
    {
        var purge = false;
        switch (Options.LogPurgeStrategy)
        {
            case LogPurgeStrategyEnum.AfterActivitySuccess:
                purge = Instance.State == ActivityStateEnum.Success;
                break;
            case LogPurgeStrategyEnum.None:
            case LogPurgeStrategyEnum.AfterWorkflowSuccess:
            case LogPurgeStrategyEnum.AfterWorkflowReturn:
            case LogPurgeStrategyEnum.AfterWorkflowComplete:
                break;
            default:
                throw new FulcrumAssertionFailedException(
                    $"Unexpected {nameof(LogPurgeStrategyEnum)}: {Options.LogPurgeStrategy}",
                    CodeLocation.AsString());
        }

        foreach (var logCreate in ActivityInformation.Logs)
        {
            if (purge && (int)logCreate.SeverityLevel <= (int)Options.LogPurgeThreshold) continue;
            ActivityInformation.Workflow.Logs.Add(logCreate);
        }
    }

    public async Task SafeAlertExceptionAsync(CancellationToken cancellationToken)
    {
        if (Instance.State != ActivityStateEnum.Failed) return;
        if (Instance.ExceptionCategory == null)
        {
            await this.LogErrorAsync($"Instance.ExceptionCategory unexpectedly was null. {CodeLocation.AsString()}",
                Instance, cancellationToken);
            return;
        }

        if (Instance.ExceptionAlertHandled.HasValue &&
            Instance.ExceptionAlertHandled.Value) return;

        if (Options.ExceptionAlertHandler == null) return;

        var alert = new ActivityExceptionAlert
        {
            Activity = this,
            ExceptionCategory = Instance.ExceptionCategory!.Value,
            ExceptionFriendlyMessage =
                Instance.ExceptionFriendlyMessage,
            ExceptionTechnicalMessage =
                Instance.ExceptionTechnicalMessage
        };
        try
        {
            var handled =
                await Options.ExceptionAlertHandler(alert, cancellationToken);
            Instance.ExceptionAlertHandled = handled;
        }
        catch (Exception)
        {
            // We will try again next reentry.
        }
    }

    protected async Task MaybeRaiseAsync<TActivity>(ISemaphoreSupport semaphoreSupport,
        ActivityMethodAsync<TActivity> whenWaitingAsync, CancellationToken cancellationToken)
    where TActivity : class, IActivity
    {
        CancellationToken ct;
        if (semaphoreSupport == null) return;
        try
        {
            await semaphoreSupport.RaiseAsync(cancellationToken);
        }
        catch (RequestPostponedException)
        {
            if (whenWaitingAsync == null) throw;
            await LogicExecutor.ExecuteWithoutReturnValueAsync(
                token => whenWaitingAsync(this as TActivity, token),
                "Already locked",
                ct);
            throw;
        }
    }

    protected async Task MaybeLowerAsync(ISemaphoreSupport semaphoreSupport, CancellationToken cancellationToken)
    {
        if (semaphoreSupport == null) return;
        await semaphoreSupport.LowerAsync(cancellationToken);
    }

    public virtual Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        throw new ActivityFailedException(ActivityExceptionCategoryEnum.WorkflowCapabilityError,
            $"The WF code for activity {this.ToLogString()} doesn't implement Task {nameof(ExecuteAsync)}().",
            "There was a problem with the Workflow Engine, please contact the developer of this workflow");
    }

    /// <inheritdoc />
    public virtual async Task<ISpawnedActivity> SpawnAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            WorkflowStatic.Context.ExecutionBackgroundStyle = ActivityAction.BackgroundStyleEnum.Spawn;
            await ExecuteAsync(cancellationToken);
            return this;
        }
        catch (RequestPostponedException)
        {
            return this;
        }
    }

    /// <inheritdoc />
    public virtual async Task AwaitAsync(CancellationToken cancellationToken = default)
    {
        await ExecuteAsync(cancellationToken);
    }
}

/// <inheritdoc cref="Activity" />
internal abstract class Activity<TActivityReturns> : Activity, IInternalActivity<TActivityReturns>, IExecutableActivity<TActivityReturns>, ISpawnedActivity<TActivityReturns>
{
    [JsonIgnore]
    public ActivityDefaultValueMethodAsync<TActivityReturns> DefaultValueMethodAsync { get; }

    protected Activity(IActivityInformation activityInformation,
        ActivityDefaultValueMethodAsync<TActivityReturns> defaultValueMethodAsync)
        : base(activityInformation)
    {
        DefaultValueMethodAsync = defaultValueMethodAsync;
    }

    public TActivityReturns GetResult()
    {
        return ActivityInformation.Workflow.GetActivityResult<TActivityReturns>(ActivityInstanceId);
    }

    public new virtual Task<TActivityReturns> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        throw new ActivityFailedException(ActivityExceptionCategoryEnum.WorkflowCapabilityError,
            $"The WF code for activity {this.ToLogString()} doesn't implement Task<{nameof(TActivityReturns)}> {nameof(ExecuteAsync)}().",
            "There was a problem with the Workflow Engine, please contact the developer of this workflow");
    }

    /// <inheritdoc />
    public new virtual async Task<ISpawnedActivity<TActivityReturns>> SpawnAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            WorkflowStatic.Context.ExecutionBackgroundStyle = ActivityAction.BackgroundStyleEnum.Spawn;
            await ExecuteAsync(cancellationToken);
            return this;
        }
        catch (RequestPostponedException)
        {
            return this;
        }
    }

    /// <inheritdoc />
    public new virtual async Task<TActivityReturns> AwaitAsync(CancellationToken cancellationToken = default)
    {
        var result = await ExecuteAsync(cancellationToken);
        return result;
    }
}