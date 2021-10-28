using Nexus.Link.AsyncManager.Sdk;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.MethodSupport;
using Nexus.Link.WorkflowEngine.Sdk.Persistence;
using Nexus.Link.WorkflowEngine.Sdk.WorkflowLogic;

namespace WorkflowEngine.Sdk.UnitTests.WorkflowLogic.Support
{
    internal class ActivityFlowMock : IInternalActivityFlow
    {
        public IWorkflowVersion WorkflowVersion { get; set; }
        public  WorkflowPersistence WorkflowPersistence { get; set; }
        protected IWorkflowCapability WorkflowCapability { get; set; }
        protected IAsyncRequestClient AsyncRequestClient { get; set; }
        public string ActivityFormId { get; set; }
        public MethodHandler MethodHandler { get; set; }
        public string FormTitle { get; set; }
        public ActivityFailUrgencyEnum FailUrgency { get; set; }

        public ActivityFlowMock(IWorkflowVersion workflowVersion, IWorkflowCapability workflowCapability,
            IAsyncRequestClient asyncRequestClient,
            WorkflowPersistence workflowPersistence, string formTitle, string activityFormId)
        {
            WorkflowVersion = workflowVersion;
            WorkflowPersistence = workflowPersistence;
            WorkflowCapability = workflowCapability;
            AsyncRequestClient = asyncRequestClient;
            ActivityFormId = activityFormId;
            FormTitle = formTitle;
            MethodHandler = new MethodHandler(formTitle);
        }
    }
}