using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.State;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.MethodSupport;
using Nexus.Link.WorkflowEngine.Sdk.Persistence;

namespace WorkflowEngine.Sdk.UnitTests.WorkflowLogic.Support
{
    internal class ActivityFlowMock : IInternalActivityFlow
    {
        /// <inheritdoc />
        public IWorkflowVersion WorkflowVersion { get; set; }

        /// <inheritdoc />
        public  WorkflowPersistence WorkflowPersistence { get; set; }

        /// <inheritdoc />
        public string ActivityFormId { get; set; }

        /// <inheritdoc />
        public MethodHandler MethodHandler { get; set; }

        /// <inheritdoc />
        public string FormTitle { get; set; }

        /// <inheritdoc />
        public ActivityFailUrgencyEnum FailUrgency { get; set; }

        /// <inheritdoc />
        public int Position { get; set; }


        public ActivityFlowMock(IWorkflowVersion workflowVersion,
            WorkflowPersistence workflowPersistence, string formTitle, string activityFormId, int position)
        {
            WorkflowVersion = workflowVersion;
            WorkflowPersistence = workflowPersistence;

            ActivityFormId = activityFormId;
            FormTitle = formTitle;
            MethodHandler = new MethodHandler(formTitle);
            Position = position;
        }
    }
}