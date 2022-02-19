using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Crud.Model;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Extensions.State;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Model;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Support;
using Nexus.Link.WorkflowEngine.Sdk.Support;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Logic
{
    /// <summary>
    /// Handles the execution of a workflow. This is only on the top level, it is <see cref="ActivityExecutor"/> that handles the actual activity execution.
    /// </summary>
    internal class WorkflowExecutor : IWorkflowLogger
    {
        private readonly MethodHandler _methodHandler;
        private Lock<string> _workflowDistributedLock;
        protected readonly WorkflowInformation WorkflowInformation;
        protected WorkflowCache WorkflowCache;

        public WorkflowExecutor(IWorkflowImplementationBase workflowImplementation)
        {
            WorkflowInformation =
                new WorkflowInformation(workflowImplementation);
            _methodHandler = new MethodHandler(workflowImplementation.WorkflowContainer.WorkflowFormTitle);
        }

        public ActivityOptions DefaultActivityOptions => WorkflowInformation.DefaultActivityOptions;

        public T GetArgument<T>(string name)
        {
            return _methodHandler.GetArgument<T>(name);
        }

        public IActivity GetCurrentParentActivity()
        {
            return WorkflowCache.GetCurrentParentActivity();
        }

        protected async Task PrepareBeforeExecutionAsync(CancellationToken cancellationToken)
        {
            FulcrumAssert.IsNotNullOrWhiteSpace(FulcrumApplication.Context.ExecutionId, CodeLocation.AsString());
            WorkflowStatic.Context.WorkflowInstanceId = FulcrumApplication.Context.ExecutionId.ToGuidString();

            WorkflowInformation.InstanceId = WorkflowStatic.Context.WorkflowInstanceId;

            WorkflowCache = new WorkflowCache(WorkflowInformation);
            await WorkflowCache.LoadAsync(cancellationToken);
            if (WorkflowCache.InstanceExists())
            {
                _workflowDistributedLock = await WorkflowInformation.WorkflowCapabilities.StateCapability.WorkflowInstance.ClaimDistributedLockAsync(
                    WorkflowInformation.InstanceId, null, null, cancellationToken);
            }
            WorkflowCache.Form.CapabilityName = WorkflowInformation.CapabilityName;
            WorkflowCache.Form.Title = WorkflowInformation.FormTitle;
            WorkflowCache.Version.MinorVersion = WorkflowInformation.MinorVersion;
            WorkflowCache.Instance.State = WorkflowStateEnum.Executing;
            WorkflowCache.Instance.Title = WorkflowInformation.InstanceTitle;
            await WorkflowCache.SaveAsync(cancellationToken);
            // TODO: Unit test for cancelled
            if (WorkflowCache.Instance.CancelledAt != null)
            {
                throw new WorkflowFailedException(
                    ActivityExceptionCategoryEnum.BusinessError,
                    $"This workflow was manually marked for cancelling at {WorkflowCache.Instance.CancelledAt.Value.ToLogString()}.",
                $"This workflow was manually marked for cancelling at {WorkflowCache.Instance.CancelledAt.Value.ToLogString()}.");
            }
        }

        protected void MarkWorkflowAsSuccess<TResult>(TResult result)
        {
            WorkflowCache.Instance.ResultAsJson = JsonConvert.SerializeObject(result);
            MarkWorkflowAsSuccess();
        }

        protected void MarkWorkflowAsSuccess()
        {
            WorkflowCache.Instance.State = WorkflowStateEnum.Success;
            WorkflowCache.Instance.FinishedAt = DateTimeOffset.UtcNow;
        }

        protected async Task AfterExecutionAsync(CancellationToken cancellationToken)
        {
            WorkflowCache.AggregateActivityInformation();
            await WorkflowCache.SaveAsync(cancellationToken);
            if (_workflowDistributedLock != null)
            {
                await WorkflowInformation.WorkflowCapabilities.StateCapability.WorkflowInstance.ReleaseDistributedLockAsync(
                    _workflowDistributedLock.ItemId, _workflowDistributedLock.LockId, cancellationToken);
            }
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
        public void SetDefaultExceptionAlertHandler(ActivityExceptionAlertHandler alertHandler)
        {
            WorkflowInformation.DefaultActivityOptions.ExceptionAlertHandler = alertHandler;
        }

        [Obsolete("Please use DefaultActivityOptions.AsyncRequestPriority. Compilation warning since 2021-11-19.")]
        public void SetDefaultAsyncRequestPriority(double priority)
        {
            WorkflowInformation.DefaultActivityOptions.AsyncRequestPriority = priority;
        }

        public async Task<TWorkflowResult> ExecuteAsync<TWorkflowResult>(WorkflowImplementation<TWorkflowResult> workflowImplementation, CancellationToken cancellationToken)
        {
            await PrepareBeforeExecutionAsync(cancellationToken);
            WorkflowStatic.Context.ExecutionIsAsynchronous = true;
            try
            {
                await this.LogInformationAsync($"Begin workflow execution", WorkflowCache.Instance, cancellationToken);
                var result = await workflowImplementation.ExecuteWorkflowAsync(cancellationToken);
                MarkWorkflowAsSuccess(result);
                await this.LogInformationAsync($"Workflow successful", result, cancellationToken);
                return result;
            }
            catch (Exception e)
            {
                throw await HandleAndCreateAsync(e, cancellationToken);
            }
            finally
            {
                try
                {
                    await AfterExecutionAsync(cancellationToken);
                }
                finally
                {
                    await this.LogInformationAsync($"End workflow execution", WorkflowCache.Instance, cancellationToken);
                    await PurgeLogsAsync(cancellationToken);
                }
            }
        }

        public async Task PurgeLogsAsync(CancellationToken cancellationToken)
        {
            var purge = false;
            var workflowInstance = WorkflowCache.Instance;
            FulcrumAssert.IsNotNull(workflowInstance, CodeLocation.AsString());
            switch (DefaultActivityOptions.LogPurgeStrategy)
            {
                case LogPurgeStrategyEnum.AfterActivitySuccess:
                    purge = workflowInstance.IsComplete;
                    break;
                case LogPurgeStrategyEnum.AfterWorkflowSuccess:
                    purge = workflowInstance.State == WorkflowStateEnum.Success;
                    break;
                case LogPurgeStrategyEnum.AfterWorkflowReturn:
                    purge = workflowInstance.State == WorkflowStateEnum.Success || workflowInstance.State == WorkflowStateEnum.Failed;
                    break;
                case LogPurgeStrategyEnum.AfterWorkflowComplete:
                    purge = workflowInstance.IsComplete;
                    break;
                case LogPurgeStrategyEnum.None:
                    break;
                default:
                    throw new FulcrumAssertionFailedException(
                        $"Unexpected {nameof(LogPurgeStrategyEnum)}: {DefaultActivityOptions.LogPurgeStrategy}", 
                        CodeLocation.AsString());
            }

            if (!purge) return;
            await WorkflowInformation.WorkflowCapabilities.StateCapability.Log.DeleteWorkflowChildrenAsync(workflowInstance.Id, DefaultActivityOptions.LogPurgeThreshold, cancellationToken);
        }

        public async Task ExecuteAsync(WorkflowImplementation workflowImplementation, CancellationToken cancellationToken)
        {
            await PrepareBeforeExecutionAsync(cancellationToken);
            WorkflowStatic.Context.ExecutionIsAsynchronous = true;
            try
            {
                await this.LogInformationAsync($"Begin workflow execution", WorkflowCache.Instance, cancellationToken);
                await workflowImplementation.ExecuteWorkflowAsync(cancellationToken);
                MarkWorkflowAsSuccess();
                await this.LogInformationAsync($"Workflow successful", null, cancellationToken);
            }
            catch (Exception e)
            {
                throw await HandleAndCreateAsync(e, cancellationToken);
            }
            finally
            {
                try
                {
                    await AfterExecutionAsync(cancellationToken);
                }
                finally
                {
                    await this.LogInformationAsync($"End workflow execution", WorkflowCache.Instance, cancellationToken);
                }
            }
        }

        private async Task<Exception> HandleAndCreateAsync(Exception exception, CancellationToken cancellationToken)
        {
            if (exception is not ExceptionTransporter exceptionTransporter)
            {
                await UnexpectedExceptionAsync(exception);
                return new RequestPostponedException();
            }

            var innerException = exceptionTransporter.InnerException;

            switch (innerException)
            {
                case WorkflowFailedException wfe:
                    WorkflowCache.Instance.State = WorkflowStateEnum.Failed;
                    WorkflowCache.Instance.FinishedAt = DateTimeOffset.UtcNow;
                    WorkflowCache.Instance.ExceptionTechnicalMessage = wfe.TechnicalMessage;
                    WorkflowCache.Instance.ExceptionFriendlyMessage = wfe.FriendlyMessage;
                    return new FulcrumCancelledException(wfe.TechnicalMessage)
                    {
                        FriendlyMessage = wfe.FriendlyMessage
                    };
                case RequestPostponedException:
                    return innerException;
                case WorkflowFastForwardBreakException:
                    return innerException;
                case FulcrumTryAgainException:
                    return new RequestPostponedException
                    {
                        TryAgain = true
                    };
                default:
                    await UnexpectedExceptionAsync(innerException);
                    return new RequestPostponedException();
            }

            async Task UnexpectedExceptionAsync(Exception unexpected)
            {
                var technicalMessage = $"Workflow engine error. Unexpected exception of type {unexpected?.GetType().Name}: {unexpected?.Message}";
                await this.LogCriticalAsync(technicalMessage, exception, cancellationToken);
                WorkflowCache.Instance.State = WorkflowStateEnum.Halted;
                WorkflowCache.Instance.ExceptionTechnicalMessage =
                    technicalMessage;
                WorkflowCache.Instance.ExceptionFriendlyMessage =
                    "The workflow engine failed; it encountered an unexpected exception";
            }
        }

        public IActivityFlow<TActivityReturns> CreateActivity<TActivityReturns>(int position, string id)
        {
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));

            WorkflowStatic.Context.LatestActivity = WorkflowCache.LatestActivity;

            return new ActivityFlow<TActivityReturns>(WorkflowInformation, WorkflowCache,
                position, id.ToGuidString());
        }

        public IActivityFlow CreateActivity(int position, string id)
        {
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));

            WorkflowStatic.Context.LatestActivity = WorkflowCache.LatestActivity;

            return new ActivityFlow(WorkflowInformation, WorkflowCache, position, id.ToGuidString());
        }

        /// <inheritdoc />
        public async Task LogAtLevelAsync(LogSeverityLevel severityLevel, string message, object data = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                FulcrumAssert.IsNotNull(WorkflowInformation.WorkflowCapabilities.StateCapability, CodeLocation.AsString());
                FulcrumAssert.IsNotNullOrWhiteSpace(message, nameof(message));
                if ((int) severityLevel < (int) DefaultActivityOptions.LogCreateThreshold) return;
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
                await WorkflowInformation.WorkflowCapabilities.StateCapability.Log.CreateAsync(log, cancellationToken);
            }
            catch (Exception)
            {
                if (FulcrumApplication.IsInDevelopment) throw;
                // Ignore logging problems when not in development mode.
            }
        }
    }
}