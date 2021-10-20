using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.AsyncManager.Sdk;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Support;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Web.Pipe;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.MethodSupport;
using Nexus.Link.WorkflowEngine.Sdk.Model;
using Nexus.Link.WorkflowEngine.Sdk.Temporary;

namespace Nexus.Link.WorkflowEngine.Sdk.WorkflowLogic
{
    public abstract class WorkflowVersionBase : IWorkflowVersionBase
    {
        private readonly IWorkflowCapability _workflowCapability;
        public int MajorVersion { get; }
        public int MinorVersion { get; }
        private readonly WorkflowInformation _workflowInformation;
        private readonly IAsyncRequestClient _asyncRequestClient;

        protected WorkflowVersionBase(int majorVersion, int minorVersion,
            WorkflowVersionCollection workflowVersionCollection)
        {
            var workflowVersionCollection1 = workflowVersionCollection;
            _workflowCapability = workflowVersionCollection.Capability;
            _asyncRequestClient = workflowVersionCollection.AsyncRequestClient;
            MajorVersion = majorVersion;
            MinorVersion = minorVersion;
            var methodHandler = new MethodHandler(workflowVersionCollection1.WorkflowFormTitle);
            _workflowInformation = new WorkflowInformation(workflowVersionCollection1.Capability, methodHandler)
            {
                CapabilityName = workflowVersionCollection1.WorkflowCapabilityName,
                FormId = workflowVersionCollection1.WorkflowFormId,
                FormTitle = workflowVersionCollection1.WorkflowFormTitle,
                MajorVersion = MajorVersion,
                MinorVersion = MinorVersion
            };
        }

        protected void DefineParameter<T>(string name)
        {
            _workflowInformation.MethodHandler.DefineParameter<T>(name);
        }

        protected TParameter GetArgument<TParameter>(string name)
        {
            return _workflowInformation.MethodHandler.GetArgument<TParameter>(name);
        }

        protected void InternalSetParameter<TParameter>(string name, TParameter value)
        {
            _workflowInformation.MethodHandler.SetParameter(name, value);
        }

        protected abstract string GetInstanceTitle();

        protected IActivityFlow<TActivityReturns> CreateActivity<TActivityReturns>(string title, string id)
        {
            InternalContract.RequireNotNullOrWhiteSpace(title, nameof(title));
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));

            return new ActivityFlow<TActivityReturns>(this, _workflowCapability, _asyncRequestClient, _workflowInformation, title, id);
        }

        protected IActivityFlow CreateActivity(string title, string id)
        {
            InternalContract.RequireNotNullOrWhiteSpace(title, nameof(title));
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));

            return new ActivityFlow(this, _workflowCapability, _asyncRequestClient, _workflowInformation, title, id);
        }

        protected async Task InternalExecuteAsync(CancellationToken cancellationToken)
        {
            // Make sure we're on correct database version
            InternalContract.RequireNotNull(DatabasePatchSettings.DatabasePatchLevelVerifier, "You need to setup DatabasePatchSettings.DatabasePatchLevelVerifier");
            await DatabasePatchSettings.DatabasePatchLevelVerifier.VerifyDatabasePatchLevel(DatabasePatchSettings.DatabasePatchVersion, cancellationToken);

            _workflowInformation.InstanceTitle = GetInstanceTitle();
            _workflowInformation.MethodHandler.InstanceTitle = _workflowInformation.InstanceTitle;
            if (string.IsNullOrWhiteSpace(AsyncWorkflowStatic.Context.WorkflowInstanceId))
            {
                throw new FulcrumNotImplementedException($"Currently all workflows must be called via AsyncManager, because they are dependent on the request header {Constants.ExecutionIdHeaderName}.");
            }
            _workflowInformation.InstanceId = AsyncWorkflowStatic.Context.WorkflowInstanceId;
            await _workflowInformation.PersistAsync(cancellationToken);
            AsyncWorkflowStatic.Context.ExecutionIsAsynchronous = true;
        }
    }

    public abstract class WorkflowVersion<TResponse> : WorkflowVersionBase
    {
        protected WorkflowVersion(int majorVersion, int minorVersion,
            WorkflowVersionCollection workflowVersionCollection)
        :base(majorVersion, minorVersion, workflowVersionCollection)
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
            await InternalExecuteAsync(cancellationToken);
            return await ExecuteWorkflowAsync(cancellationToken);
        }

        protected abstract Task<TResponse> ExecuteWorkflowAsync(CancellationToken cancellationToken);
    }

    public abstract class WorkflowVersion : WorkflowVersionBase
    {
        protected WorkflowVersion(int majorVersion, int minorVersion,
            WorkflowVersionCollection workflowVersionCollection)
        :base(majorVersion, minorVersion, workflowVersionCollection)
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
            await InternalExecuteAsync(cancellationToken);
            await ExecuteWorkflowAsync(cancellationToken);
        }

        protected abstract Task ExecuteWorkflowAsync(CancellationToken cancellationToken);
    }
}