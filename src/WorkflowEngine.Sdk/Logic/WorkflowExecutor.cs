using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.State;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Crud.Model;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.Libraries.Web.Pipe;
using Nexus.Link.WorkflowEngine.Sdk.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Extensions.State;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Persistence;
using Nexus.Link.WorkflowEngine.Sdk.Support;
using Nexus.Link.WorkflowEngine.Sdk.Support.Method;
using Nexus.Link.WorkflowEngine.Sdk.Temporary;

namespace Nexus.Link.WorkflowEngine.Sdk.Logic
{
    public class WorkflowExecutor : IWorkflowLogger
    {
        private readonly MethodHandler _methodHandler;
        private Lock<string> _workflowDistributedLock;
        protected readonly WorkflowInformation WorkflowInformation;
        protected WorkflowCache WorkflowCache;

        public WorkflowExecutor(IWorkflowImplementationBase workflowImplementation)
        {
            WorkflowInformation =
                new WorkflowInformation(workflowImplementation);
            _methodHandler = new MethodHandler(workflowImplementation.WorkflowVersions.WorkflowFormTitle);
        }

        public ActivityOptions DefaultActivityOptions => WorkflowInformation.DefaultActivityOptions;

        public T GetArgument<T>(string name)
        {
            return _methodHandler.GetArgument<T>(name);
        }

        protected async Task PrepareBeforeExecutionAsync(CancellationToken cancellationToken)
        {
            FulcrumAssert.IsNotNullOrWhiteSpace(FulcrumApplication.Context.ExecutionId, CodeLocation.AsString());
            WorkflowStatic.Context.WorkflowInstanceId = FulcrumApplication.Context.ExecutionId;

            // If service runs directly with database connection, make sure we're on correct database version
#pragma warning disable CS0618
            if (DatabasePatchSettings.DatabasePatchLevelVerifier != null)
            {
                await DatabasePatchSettings.DatabasePatchLevelVerifier.VerifyDatabasePatchLevel(DatabasePatchSettings.DatabasePatchVersion, cancellationToken);
            }
#pragma warning restore CS0618

            WorkflowInformation.InstanceId = WorkflowStatic.Context.WorkflowInstanceId;

            WorkflowCache = new WorkflowCache(WorkflowInformation);
            await WorkflowCache.LoadAsync(cancellationToken);
            if (WorkflowCache.InstanceExists())
            {
                _workflowDistributedLock = await WorkflowInformation.WorkflowCapability.WorkflowInstance.ClaimDistributedLockAsync(
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
                await WorkflowInformation.WorkflowCapability.WorkflowInstance.ReleaseDistributedLockAsync(
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
                await WorkflowInformation.LogInformationAsync($"Begin workflow execution", WorkflowCache.Instance, cancellationToken);
                var result = await workflowImplementation.ExecuteWorkflowAsync(cancellationToken);
                MarkWorkflowAsSuccess(result);
                await WorkflowInformation.LogInformationAsync($"Workflow successful", result, cancellationToken);
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
                    await WorkflowInformation.LogInformationAsync($"End workflow execution", WorkflowCache.Instance, cancellationToken);
                }
            }
        }

        public async Task ExecuteAsync(WorkflowImplementation workflowImplementation, CancellationToken cancellationToken)
        {
            await PrepareBeforeExecutionAsync(cancellationToken);
            WorkflowStatic.Context.ExecutionIsAsynchronous = true;
            try
            {
                await WorkflowInformation.LogInformationAsync($"Begin workflow execution", WorkflowCache.Instance, cancellationToken);
                await workflowImplementation.ExecuteWorkflowAsync(cancellationToken);
                MarkWorkflowAsSuccess();
                await WorkflowInformation.LogInformationAsync($"Workflow successful", null, cancellationToken);
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
                    await WorkflowInformation.LogInformationAsync($"End workflow execution", WorkflowCache.Instance, cancellationToken);
                }
            }
        }

        private async Task<Exception> HandleAndCreateAsync(Exception exception, CancellationToken cancellationToken)
        {
            if (!(exception is ExceptionTransporter exceptionTransporter))
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
                await WorkflowInformation.LogCriticalAsync(technicalMessage, exception, cancellationToken);
                WorkflowCache.Instance.State = WorkflowStateEnum.Halted;
                WorkflowCache.Instance.ExceptionTechnicalMessage =
                    technicalMessage;
                WorkflowCache.Instance.ExceptionFriendlyMessage =
                    $"The workflow engine failed; it encountered an unexpected exception";
            }
        }

        public IActivityFlow<TActivityReturns> CreateActivity<TActivityReturns>(int position, string title, string id)
        {
            InternalContract.RequireNotNullOrWhiteSpace(title, nameof(title));
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));

            WorkflowStatic.Context.LatestActivity = WorkflowCache.LatestActivity;

            return new ActivityFlow<TActivityReturns>(WorkflowInformation, WorkflowCache,
                position, title, id.ToLowerInvariant());
        }

        public IActivityFlow CreateActivity(int position, string title, string id)
        {
            InternalContract.RequireNotNullOrWhiteSpace(title, nameof(title));
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));

            WorkflowStatic.Context.LatestActivity = WorkflowCache.LatestActivity;

            return new ActivityFlow(WorkflowInformation, WorkflowCache, position, title, id.ToLowerInvariant());
        }

        /// <inheritdoc />
        public Task LogAtLevelAsync(LogSeverityLevel severityLevel, string message, object data = null,
            CancellationToken cancellationToken = default)
        {
            return WorkflowInformation.LogAtLevelAsync(severityLevel, message, data, cancellationToken);
        }
    }
}