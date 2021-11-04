using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nexus.Link.AsyncManager.Sdk;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.Configuration;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.State;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Crud.Model;
using Nexus.Link.Libraries.Web.Pipe;
using Nexus.Link.WorkflowEngine.Sdk.ActivityLogic;
using Nexus.Link.WorkflowEngine.Sdk.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.MethodSupport;
using Nexus.Link.WorkflowEngine.Sdk.Persistence;
using Nexus.Link.WorkflowEngine.Sdk.Support;
using Nexus.Link.WorkflowEngine.Sdk.Temporary;

namespace Nexus.Link.WorkflowEngine.Sdk.WorkflowLogic
{
    public abstract class WorkflowVersionBase : IWorkflowVersion
    {
        private readonly WorkflowInformation _workflowInformation;
        private readonly WorkflowVersionCollection _workflowVersionCollection;
        private readonly MethodHandler _methodHandler;
        private WorkflowCache _workflowCache;
        private Lock<string> _workflowDistributedLock;

        /// <inheritdoc />
        public int MajorVersion => _workflowInformation.MajorVersion;

        /// <inheritdoc />
        public int MinorVersion => _workflowInformation.MinorVersion;

        protected WorkflowVersionBase(int majorVersion, int minorVersion,
            WorkflowVersionCollection workflowVersionCollection)
        {
            _workflowVersionCollection = workflowVersionCollection;
            _workflowInformation = new WorkflowInformation
            {
                WorkflowCapability = workflowVersionCollection.Capability,
                FormId = workflowVersionCollection.WorkflowFormId,
                FormTitle = workflowVersionCollection.WorkflowFormTitle,
                CapabilityName = workflowVersionCollection.WorkflowCapabilityName,
                MajorVersion = majorVersion,
                MinorVersion = minorVersion,
                InstanceId = AsyncWorkflowStatic.Context.WorkflowInstanceId
            };
            _methodHandler = new MethodHandler(workflowVersionCollection.WorkflowFormTitle);
        }

        protected void DefineParameter<T>(string name)
        {
            _methodHandler.DefineParameter<T>(name);
        }

        protected TParameter GetArgument<TParameter>(string name)
        {
            return _methodHandler.GetArgument<TParameter>(name);
        }

        protected void InternalSetParameter<TParameter>(string name, TParameter value)
        {
            _methodHandler.SetParameter(name, value);
        }

        protected abstract string GetInstanceTitle();

        protected IActivityFlow<TActivityReturns> CreateActivity<TActivityReturns>(int position, string title, string id)
        {
            InternalContract.RequireNotNullOrWhiteSpace(title, nameof(title));
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));

            AsyncWorkflowStatic.Context.LatestActivityInstanceId = _workflowCache.LatestActivityInstanceId;

            return new ActivityFlow<TActivityReturns>(_workflowInformation.WorkflowCapability, _workflowVersionCollection.AsyncRequestClient, _workflowCache,
                this, position, title, id.ToLowerInvariant());
        }

        protected IActivityFlow CreateActivity(int position, string title, string id)
        {
            InternalContract.RequireNotNullOrWhiteSpace(title, nameof(title));
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));

            AsyncWorkflowStatic.Context.LatestActivityInstanceId = _workflowCache.LatestActivityInstanceId;

            return new ActivityFlow(_workflowInformation.WorkflowCapability, _workflowVersionCollection.AsyncRequestClient, _workflowCache,
                this, position, title, id.ToLowerInvariant());
        }

        internal async Task PrepareBeforeExecutionAsync(CancellationToken cancellationToken)
        {
            // If service runs directly with database connection, make sure we're on correct database version
#pragma warning disable CS0618
            if (DatabasePatchSettings.DatabasePatchLevelVerifier != null)
            {
                await DatabasePatchSettings.DatabasePatchLevelVerifier.VerifyDatabasePatchLevel(DatabasePatchSettings.DatabasePatchVersion, cancellationToken);
            }
#pragma warning restore CS0618

            if (string.IsNullOrWhiteSpace(AsyncWorkflowStatic.Context.WorkflowInstanceId))
            {
                throw new FulcrumNotImplementedException($"Currently all workflows must be called via AsyncManager, because they are dependent on the request header {Constants.ExecutionIdHeaderName}.");
            }

            _workflowInformation.InstanceTitle = GetInstanceTitle();
            _workflowCache = new WorkflowCache(_workflowInformation);
            await _workflowCache.LoadAsync(cancellationToken);
            if (_workflowCache.InstanceExists())
            {
                _workflowDistributedLock = await _workflowInformation.WorkflowCapability.WorkflowInstance.ClaimDistributedLockAsync(
                    _workflowInformation.InstanceId, null, null, cancellationToken);
            }
            _workflowCache.Form.CapabilityName = _workflowVersionCollection.WorkflowCapabilityName;
            _workflowCache.Form.Title = _workflowVersionCollection.WorkflowFormTitle;
            _workflowCache.Version.MinorVersion = _workflowInformation.MinorVersion;
            _workflowCache.Instance.State = WorkflowStateEnum.Executing;
            _workflowCache.Instance.Title = GetInstanceTitle();
            await _workflowCache.SaveAsync(cancellationToken);
            // TODO: Unit test for cancelled
            if (_workflowCache.Instance.CancelledAt != null)
            {
                throw new FulcrumCancelledException(
                    $"This workflow was manually marked for cancelling at {_workflowCache.Instance.CancelledAt.Value.ToLogString()}.");
            }
        }

        internal void MarkWorkflowAsSuccess<TResult>(TResult result)
        {
            _workflowCache.Instance.ResultAsJson = JsonConvert.SerializeObject(result);
            MarkWorkflowAsSuccess();
        }

        internal bool HandleAndReportIfRethrow(Exception e)
        {
            switch (e)
            {
                case HandledRequestPostponedException:
                case FulcrumCancelledException:
                    return true;
                case WorkflowFailedException wfe:
                    _workflowCache.Instance.State = WorkflowStateEnum.Failed;
                    _workflowCache.Instance.FinishedAt = DateTimeOffset.UtcNow;
                    _workflowCache.Instance.ExceptionTechnicalMessage = wfe.TechnicalMessage;
                    _workflowCache.Instance.ExceptionFriendlyMessage = wfe.FriendlyMessage;
                    throw new FulcrumCancelledException(wfe.TechnicalMessage);
                default:
                    _workflowCache.Instance.State = WorkflowStateEnum.Halted;
                    _workflowCache.Instance.ExceptionTechnicalMessage = $"Unexpected exception: {e}";
                    _workflowCache.Instance.ExceptionFriendlyMessage = $"Unexpected exception: {e.Message}";
                    return false;
            }
        }

        internal void MarkWorkflowAsSuccess()
        {
            _workflowCache.Instance.State = WorkflowStateEnum.Success;
            _workflowCache.Instance.FinishedAt = DateTimeOffset.UtcNow;
        }

        internal async Task AfterExecutionAsync(CancellationToken cancellationToken)
        {
            _workflowCache.AggregateActivityInformation();
            await _workflowCache.SaveAsync(cancellationToken);
            if (_workflowDistributedLock != null)
            {
                await _workflowInformation.WorkflowCapability.WorkflowInstance.ReleaseDistributedLockAsync(
                    _workflowDistributedLock.ItemId, _workflowDistributedLock.LockId, cancellationToken);
            }
        }
    }

    public abstract class WorkflowVersion<TResponse> : WorkflowVersionBase
    {
        protected WorkflowVersion(int majorVersion, int minorVersion,
            WorkflowVersionCollection workflowVersionCollection)
        : base(majorVersion, minorVersion, workflowVersionCollection)
        {
        }

        public abstract WorkflowVersion<TResponse> CreateWorkflowInstance();

        public WorkflowVersion<TResponse> SetParameter<TParameter>(string name, TParameter value)
        {
            InternalSetParameter(name, value);
            return this;
        }

        public async Task<TResponse> ExecuteAsync(CancellationToken cancellationToken)
        {
            await PrepareBeforeExecutionAsync(cancellationToken);
            AsyncWorkflowStatic.Context.ExecutionIsAsynchronous = true;
            try
            {
                var result = await ExecuteWorkflowAsync(cancellationToken);
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

        protected abstract Task<TResponse> ExecuteWorkflowAsync(CancellationToken cancellationToken);
    }

    public abstract class WorkflowVersion : WorkflowVersionBase
    {
        protected WorkflowVersion(int majorVersion, int minorVersion,
            WorkflowVersionCollection workflowVersionCollection)
        : base(majorVersion, minorVersion, workflowVersionCollection)
        {
        }

        public abstract WorkflowVersion CreateWorkflowInstance();


        public WorkflowVersion SetParameter<TParameter>(string name, TParameter value)
        {
            InternalSetParameter(name, value);
            return this;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await PrepareBeforeExecutionAsync(cancellationToken);
            AsyncWorkflowStatic.Context.ExecutionIsAsynchronous = true;
            try
            {
                await ExecuteWorkflowAsync(cancellationToken);
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

        protected abstract Task ExecuteWorkflowAsync(CancellationToken cancellationToken);
    }
}