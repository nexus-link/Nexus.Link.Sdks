using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.State;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Crud.Model;
using Nexus.Link.Libraries.Web.Pipe;
using Nexus.Link.WorkflowEngine.Sdk.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Persistence;
using Nexus.Link.WorkflowEngine.Sdk.Support;
using Nexus.Link.WorkflowEngine.Sdk.Support.Method;
using Nexus.Link.WorkflowEngine.Sdk.Temporary;

namespace Nexus.Link.WorkflowEngine.Sdk.Logic
{
    public class WorkflowExecutor
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

        public T GetArgument<T>(string name)
        {
            return _methodHandler.GetArgument<T>(name);
        }
        
        protected async Task PrepareBeforeExecutionAsync(CancellationToken cancellationToken)
        {
            // If service runs directly with database connection, make sure we're on correct database version
#pragma warning disable CS0618
            if (DatabasePatchSettings.DatabasePatchLevelVerifier != null)
            {
                await DatabasePatchSettings.DatabasePatchLevelVerifier.VerifyDatabasePatchLevel(DatabasePatchSettings.DatabasePatchVersion, cancellationToken);
            }
#pragma warning restore CS0618

            if (string.IsNullOrWhiteSpace(WorkflowStatic.Context.WorkflowInstanceId))
            {
                throw new FulcrumNotImplementedException($"Currently all workflows must be called via AsyncManager, because they are dependent on the request header {Constants.ExecutionIdHeaderName}.");
            }

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
                throw new FulcrumCancelledException(
                    $"This workflow was manually marked for cancelling at {WorkflowCache.Instance.CancelledAt.Value.ToLogString()}.");
            }
        }

        protected void MarkWorkflowAsSuccess<TResult>(TResult result)
        {
            WorkflowCache.Instance.ResultAsJson = JsonConvert.SerializeObject(result);
            MarkWorkflowAsSuccess();
        }

        protected bool HandleAndReportIfRethrow(Exception e)
        {
            switch (e)
            {
                case HandledRequestPostponedException:
                case FulcrumCancelledException:
                    return true;
                case WorkflowFailedException wfe:
                    WorkflowCache.Instance.State = WorkflowStateEnum.Failed;
                    WorkflowCache.Instance.FinishedAt = DateTimeOffset.UtcNow;
                    WorkflowCache.Instance.ExceptionTechnicalMessage = wfe.TechnicalMessage;
                    WorkflowCache.Instance.ExceptionFriendlyMessage = wfe.FriendlyMessage;
                    throw new FulcrumCancelledException(wfe.TechnicalMessage);
                default:
                    WorkflowCache.Instance.State = WorkflowStateEnum.Halted;
                    WorkflowCache.Instance.ExceptionTechnicalMessage = $"Unexpected exception: {e}";
                    WorkflowCache.Instance.ExceptionFriendlyMessage = $"Unexpected exception: {e.Message}";
                    return false;
            }
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

        public void SetDefaultFailUrgency(ActivityFailUrgencyEnum failUrgency)
        {
            WorkflowInformation.DefaultFailUrgency = failUrgency;
        }

        public void SetDefaultExceptionAlertHandler(ActivityExceptionAlertHandler alertHandler)
        {
            WorkflowInformation.DefaultExceptionAlertHandler = alertHandler;
        }

        public void SetDefaultAsyncRequestPriority(double priority)
        {
            WorkflowInformation.DefaultAsyncRequestPriority = priority;
        }

        public async Task<TWorkflowResult> ExecuteAsync<TWorkflowResult>(WorkflowImplementation<TWorkflowResult> workflowImplementation, CancellationToken cancellationToken)
        {
            await PrepareBeforeExecutionAsync(cancellationToken);
            WorkflowStatic.Context.ExecutionIsAsynchronous = true;
            try
            {
                var result = await workflowImplementation.ExecuteWorkflowAsync(cancellationToken);
                MarkWorkflowAsSuccess(result);
                return result;
            }
            catch (Exception e)
            {
                if (HandleAndReportIfRethrow(e)) throw;
                throw new HandledRequestPostponedException();
            }
            finally
            {
                await AfterExecutionAsync(cancellationToken);
            }
        }

        public async Task ExecuteAsync(WorkflowImplementation workflowImplementation, CancellationToken cancellationToken)
        {
            await PrepareBeforeExecutionAsync(cancellationToken);
            WorkflowStatic.Context.ExecutionIsAsynchronous = true;
            try
            {
                await workflowImplementation.ExecuteWorkflowAsync(cancellationToken);
                MarkWorkflowAsSuccess();
            }
            catch (Exception e)
            {
                if (HandleAndReportIfRethrow(e)) throw;
                throw new HandledRequestPostponedException();
            }
            finally
            {
                await AfterExecutionAsync(cancellationToken);
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
    }
}