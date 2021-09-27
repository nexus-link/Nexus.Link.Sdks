using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Support;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Model;

namespace Nexus.Link.WorkflowEngine.Sdk.WorkflowLogic
{
    public abstract class WorkflowVersion<TResponse>
    {
        private readonly IWorkflowCapabilityForClient _workflowCapability;
        public int MajorVersion { get; }
        public int MinorVersion { get; }
        private WorkflowInformation _workflowInformation;
        private readonly WorkflowVersionCollection _workflowVersionCollection;
        private readonly MethodHandler _methodHandler;

        protected WorkflowVersion(int majorVersion, int minorVersion,
            WorkflowVersionCollection workflowVersionCollection)
        {
            _workflowVersionCollection = workflowVersionCollection;
            _workflowCapability = workflowVersionCollection.Capability;
            MajorVersion = majorVersion;
            MinorVersion = minorVersion;
            var methodHandler = new MethodHandler(_workflowVersionCollection.WorkflowFormTitle);
            _workflowInformation = new WorkflowInformation(_workflowVersionCollection.Capability, methodHandler)
            {
                CapabilityName = _workflowVersionCollection.WorkflowCapabilityName,
                FormId = _workflowVersionCollection.WorkflowFormId,
                FormTitle = _workflowVersionCollection.WorkflowFormTitle,
                MajorVersion = MajorVersion,
                MinorVersion = MinorVersion
            };
        }

        public abstract WorkflowVersion<TResponse> CreateWorkflowInstance();

        protected void DefineParameter<T>(string name)
        {
            _workflowInformation.MethodHandler.DefineParameter<T>(name);
        }

        public WorkflowVersion<TResponse> SetParameter<TParameter>(string name, TParameter value)
        {
            _workflowInformation.MethodHandler.SetParameter(name, value);
            return this;
        }

        protected TParameter GetArgument<TParameter>(string name)
        {
            return _workflowInformation.MethodHandler.GetArgument<TParameter>(name);
        }

        public async Task<TResponse> ExecuteAsync(CancellationToken cancellationToken)
        {
            _workflowInformation.InstanceTitle = GetInstanceTitle();
            _workflowInformation.MethodHandler.InstanceTitle = _workflowInformation.InstanceTitle;
            var executionIdAsGuid = AsyncWorkflowStatic.Context.AsyncExecutionContext.ExecutionId;
            _workflowInformation.InstanceId = executionIdAsGuid.ToString();
            await _workflowInformation.PersistAsync(cancellationToken);
            return await ExecuteWorkflowAsync(cancellationToken);
        }

        protected abstract string GetInstanceTitle();

        protected abstract Task<TResponse> ExecuteWorkflowAsync(CancellationToken cancellationToken);

        protected IActivityFlow CreateActivity(string title, string id)
        {
            InternalContract.RequireNotNullOrWhiteSpace(title, nameof(title));
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));

            return new ActivityFlow(_workflowCapability, _workflowInformation, title, id);
        }
    }
}