using System;
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
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Execution;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Extensions.State;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Support;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;

/// <summary>
/// Handles the execution of a workflow. This is only on the top level, it is <see cref="ActivityExecutor"/> that handles the actual activity execution.
/// </summary>
internal class WorkflowExecutor : IWorkflowExecutor
{
    private readonly IWorkflowBeforeAndAfterExecution _beforeAndAfter;
    private readonly MethodHandler _methodHandler;
    public IWorkflowInformation WorkflowInformation { get; }

    public WorkflowExecutor(IWorkflowInformation workflowInformation, IWorkflowBeforeAndAfterExecution beforeAndAfter)
    {
        InternalContract.RequireNotNull(workflowInformation, nameof(workflowInformation));
        InternalContract.RequireNotNull(beforeAndAfter, nameof(beforeAndAfter));

        WorkflowInformation = workflowInformation;
        _beforeAndAfter = beforeAndAfter;
        _methodHandler = new MethodHandler(WorkflowInformation.FormTitle);
    }

    public T GetArgument<T>(string name)
    {
        return _methodHandler.GetArgument<T>(name);
    }

    public void DefineParameter<T>(string name)
    {
        _methodHandler.DefineParameter<T>(name);
    }

    public void SetParameter<TParameter>(string name, TParameter value)
    {
        _methodHandler.SetParameter(name, value);
    }

    [Obsolete("Please use DefaultActivityOptions.FailUrgency. Compilation warning since 2021-11-19.")]
    public void SetDefaultFailUrgency(ActivityFailUrgencyEnum failUrgency)
    {
        WorkflowInformation.DefaultActivityOptions.FailUrgency = failUrgency;
    }

    [Obsolete("Please use DefaultActivityOptions.ExceptionAlertHandler. Compilation warning since 2021-11-19.")]
    public void SetDefaultExceptionAlertHandler(ActivityExceptionAlertMethodAsync alertMethodAsync)
    {
        WorkflowInformation.DefaultActivityOptions.ExceptionAlertHandler = alertMethodAsync;
    }

    [Obsolete("Please use DefaultActivityOptions.AsyncRequestPriority. Compilation warning since 2021-11-19.")]
    public void SetDefaultAsyncRequestPriority(double priority)
    {
        WorkflowInformation.DefaultActivityOptions.AsyncRequestPriority = priority;
    }

    protected void MarkWorkflowAsSuccess<TResult>(TResult result)
    {
        WorkflowInformation.Instance.ResultAsJson = JsonConvert.SerializeObject(result);
        MarkWorkflowAsSuccess();
    }

    protected void MarkWorkflowAsSuccess()
    {
        WorkflowInformation.Instance.State = WorkflowStateEnum.Success;
        WorkflowInformation.Instance.FinishedAt = DateTimeOffset.UtcNow;
    }

    public async Task<TWorkflowResult> ExecuteAsync<TWorkflowResult>(IWorkflowImplementation<TWorkflowResult> workflowImplementation, CancellationToken cancellationToken)
    {
        try
        {
            WorkflowStatic.Context.CurrentWorkflowExecutor = this;
            await _beforeAndAfter.BeforeExecutionAsync(cancellationToken);
            WorkflowStatic.Context.WorkflowInstanceId = WorkflowInformation.InstanceId;
        }
        catch (Exception e)
        {
            throw await HandleAndConvertExceptionAsync(e, cancellationToken);
        }

        try {
            WorkflowStatic.Context.ExecutionIsAsynchronous = true;
            TWorkflowResult result;
            await this.LogVerboseAsync($"Begin Workflow {WorkflowInformation} execution", WorkflowInformation.Instance, cancellationToken);
            try
            {
                result = await workflowImplementation.ExecuteWorkflowAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                throw await StripAwayMandatoryOuterExceptionAsync(ex, cancellationToken);
            }

            MarkWorkflowAsSuccess(result);
            var totalExecution = DateTimeOffset.UtcNow.Subtract(WorkflowInformation.StartedAt);
            await this.LogInformationAsync($"Workflow {WorkflowInformation} completed successfully. Time since start: {totalExecution.ToLogString()}.", WorkflowInformation.Instance, cancellationToken);
            return result;
        }
        catch (Exception e)
        {
            throw await HandleAndConvertExceptionAsync(e, cancellationToken);
        }
        finally
        {
                await this.LogVerboseAsync($"End workflow {WorkflowInformation} execution." +
                                           $" This execution took {WorkflowInformation.TimeSinceCurrentRunStarted.Elapsed.ToLogString()}.",
                    WorkflowInformation.Instance, cancellationToken);
                await _beforeAndAfter.AfterExecutionAsync(cancellationToken);
        }
    }

    public async Task ExecuteAsync(IWorkflowImplementation workflowImplementation, CancellationToken cancellationToken)
    {
        try
        {
            WorkflowStatic.Context.CurrentWorkflowExecutor = this;
            await _beforeAndAfter.BeforeExecutionAsync(cancellationToken);
            WorkflowStatic.Context.WorkflowInstanceId = WorkflowInformation.InstanceId;
        }
        catch (Exception e)
        {
            throw await HandleAndConvertExceptionAsync(e, cancellationToken);
        }

        try
        {
            WorkflowStatic.Context.ExecutionIsAsynchronous = true;
            await this.LogVerboseAsync($"Begin Workflow {WorkflowInformation} execution", WorkflowInformation.Instance, cancellationToken);
            try
            {
                await workflowImplementation.ExecuteWorkflowAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                throw await StripAwayMandatoryOuterExceptionAsync(ex, cancellationToken);
            }
            MarkWorkflowAsSuccess();
            var totalExecution = DateTimeOffset.UtcNow.Subtract(WorkflowInformation.StartedAt);
            await this.LogInformationAsync($"Workflow {WorkflowInformation} completed successfully. Time since start: {totalExecution.ToLogString()}.", WorkflowInformation.Instance, cancellationToken);
        }
        catch (Exception e)
        {
            throw await HandleAndConvertExceptionAsync(e, cancellationToken);
        }
        finally
        {
            await _beforeAndAfter.AfterExecutionAsync(cancellationToken);
            await this.LogVerboseAsync($"End workflow {WorkflowInformation} execution." +
                                       $" This execution took {WorkflowInformation.TimeSinceCurrentRunStarted.Elapsed.ToLogString()}.", WorkflowInformation.Instance, cancellationToken);
        }
    }

    private async Task<Exception> StripAwayMandatoryOuterExceptionAsync(Exception ex, CancellationToken cancellationToken)
    {
        if (ex is WorkflowImplementationShouldNotCatchThisException exceptionTransporter && exceptionTransporter.InnerException != null)
        {
            return exceptionTransporter.InnerException!;
        }

        return await ReportUnexpectedExceptionAsync(ex, cancellationToken);
    }

    private async Task<Exception> ReportUnexpectedExceptionAsync(Exception ex, CancellationToken cancellationToken)
    {
        var technicalMessage = $"Workflow engine error. Unexpected exception: {ex}";
        await this.LogCriticalAsync(technicalMessage, ex, cancellationToken);
        WorkflowInformation.Instance.State = WorkflowStateEnum.Halted;
        WorkflowInformation.Instance.ExceptionTechnicalMessage =
            technicalMessage;
        WorkflowInformation.Instance.ExceptionFriendlyMessage =
            "The workflow engine failed; it encountered an unexpected exception";
        await this.LogWarningAsync($"Workflow {WorkflowInformation} failed: {technicalMessage}",
            new { Exception = ex, WorkflowInformation },
            cancellationToken);

        return new RequestPostponedException();
    }

    private async Task<Exception> HandleAndConvertExceptionAsync(Exception exception, CancellationToken cancellationToken)
    {
        switch (exception)
        {
            case WorkflowFailedException wfe:
                WorkflowInformation.Instance.State = WorkflowStateEnum.Failed;
                WorkflowInformation.Instance.FinishedAt = DateTimeOffset.UtcNow;
                WorkflowInformation.Instance.ExceptionTechnicalMessage = wfe.TechnicalMessage;
                WorkflowInformation.Instance.ExceptionFriendlyMessage = wfe.FriendlyMessage;
                await this.LogWarningAsync($"Workflow {WorkflowInformation} failed: {wfe.TechnicalMessage}", WorkflowInformation.Instance, cancellationToken);
                return new FulcrumCancelledException(wfe.TechnicalMessage)
                {
                    FriendlyMessage = wfe.FriendlyMessage
                };
            case RequestPostponedException:
            case WorkflowFastForwardBreakException:
                return exception;
            case FulcrumTryAgainException tryAgainException:
                return new RequestPostponedException
                {
                    TryAgain = true,
                    TryAgainAfterMinimumTimeSpan = TimeSpan.FromSeconds(tryAgainException.RecommendedWaitTimeInSeconds)
                };
            default:
                return await ReportUnexpectedExceptionAsync(exception, cancellationToken);
        }
    }

    public IActivityFlow<TActivityReturns> CreateActivity<TActivityReturns>(int position, string id)
    {
        InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));

        var activityInformation = new ActivityInformation(WorkflowInformation, position, id.ToGuidString());
        var flow = new ActivityFlow<TActivityReturns>(activityInformation);
        return flow;
    }

    public IActivityFlow CreateActivity(int position, string id)
    {
        InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));
        var activityInformation = new ActivityInformation(WorkflowInformation, position, id.ToGuidString());
        var flow = new ActivityFlow(activityInformation);
        return flow;
    }

    /// <inheritdoc />
    public async Task LogAtLevelAsync(LogSeverityLevel severityLevel, string message, object data = null,
        CancellationToken cancellationToken = default)
    {
        if (WorkflowInformation.LogService == null) return;
        try
        {
            FulcrumAssert.IsNotNullOrWhiteSpace(message, nameof(message));
            if ((int)severityLevel < (int)WorkflowInformation.DefaultActivityOptions.LogCreateThreshold) return;
            var jToken = WorkflowStatic.SafeConvertToJToken(data);
            var log = new LogCreate
            {
                WorkflowFormId = WorkflowInformation.FormId,
                WorkflowInstanceId = WorkflowInformation.InstanceId,
                ActivityFormId = null,
                SeverityLevel = severityLevel,
                Message = message,
                Data = jToken,
                TimeStamp = DateTimeOffset.UtcNow,
            };
            await WorkflowInformation.LogService.CreateAsync(log, cancellationToken);
        }
        catch (Exception)
        {
            if (FulcrumApplication.IsInDevelopment) throw;
            // Ignore logging problems when not in development mode.
        }
    }
}