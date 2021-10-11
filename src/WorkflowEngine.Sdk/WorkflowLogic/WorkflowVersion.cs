﻿using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.AsyncManager.Sdk;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Exceptions;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Support;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Web.Pipe;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.MethodSupport;
using Nexus.Link.WorkflowEngine.Sdk.Model;
using Nexus.Link.WorkflowEngine.Sdk.Temporary;

namespace Nexus.Link.WorkflowEngine.Sdk.WorkflowLogic
{
    public abstract class WorkflowVersion<TResponse>
    {
        private readonly IWorkflowCapability _workflowCapability;
        public int MajorVersion { get; }
        public int MinorVersion { get; }
        private WorkflowInformation _workflowInformation;
        private readonly WorkflowVersionCollection _workflowVersionCollection;
        private readonly MethodHandler _methodHandler;
        private readonly IAsyncRequestClient _asyncRequestClient;

        protected WorkflowVersion(int majorVersion, int minorVersion,
            WorkflowVersionCollection workflowVersionCollection)
        {
            _workflowVersionCollection = workflowVersionCollection;
            _workflowCapability = workflowVersionCollection.Capability;
            _asyncRequestClient = workflowVersionCollection.AsyncRequestClient;
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
            // Make sure we're on correct database version
            await _workflowVersionCollection.DatabasePatchLevelVerifier.VerifyDatabasePatchLevel(FulcrumApplication.Setup.Tenant, DatabasePatchLevel.Version, cancellationToken);

            _workflowInformation.InstanceTitle = GetInstanceTitle();
            _workflowInformation.MethodHandler.InstanceTitle = _workflowInformation.InstanceTitle;
            if (string.IsNullOrWhiteSpace(AsyncWorkflowStatic.Context.WorkflowInstanceId))
            {
                throw new FulcrumNotImplementedException($"Currently all workflows must be called via AsyncManager, because they are dependent on the request header {Constants.ExecutionIdHeaderName}.");
            }
            _workflowInformation.InstanceId = AsyncWorkflowStatic.Context.WorkflowInstanceId;
            await _workflowInformation.PersistAsync(cancellationToken);
            AsyncWorkflowStatic.Context.ExecutionIsAsynchronous = true;
            try
            {
                return await ExecuteWorkflowAsync(cancellationToken);
            }
            catch (PostponeException e)
            {
                // TODO: Is this a relevant exception? Will be sent to AM.
                throw new FulcrumTryAgainException();
            }
        }

        protected abstract string GetInstanceTitle();

        protected abstract Task<TResponse> ExecuteWorkflowAsync(CancellationToken cancellationToken);

        protected IActivityFlow CreateActivity(string title, string id)
        {
            InternalContract.RequireNotNullOrWhiteSpace(title, nameof(title));
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));

            return new ActivityFlow(_workflowCapability, _asyncRequestClient, _workflowInformation, title, id);
        }
    }
}