using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
using Nexus.Link.Components.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Extensions.State;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Support;
using Nexus.Link.WorkflowEngine.Sdk.Support;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;

/// <inheritdoc cref="IInternalActivity" />
internal abstract class Activity : ActivityBase, IInternalActivity
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
}

/// <inheritdoc cref="Activity" />
internal abstract class Activity<TActivityReturns> : Activity, IInternalActivity<TActivityReturns>
{
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
}