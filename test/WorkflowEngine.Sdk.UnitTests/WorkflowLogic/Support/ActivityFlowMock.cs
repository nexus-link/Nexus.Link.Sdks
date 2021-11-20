using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.State;
using Nexus.Link.WorkflowEngine.Sdk;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Persistence;
using Nexus.Link.WorkflowEngine.Sdk.Support;
using Nexus.Link.WorkflowEngine.Sdk.Support.Method;

namespace WorkflowEngine.Sdk.UnitTests.WorkflowLogic.Support
{
    internal class ActivityFlowMock : IInternalActivityFlow
    {
        /// <inheritdoc />
        public WorkflowInformation WorkflowInformation { get; set; }

        /// <inheritdoc />
        public WorkflowCache WorkflowCache { get; set; }

        /// <inheritdoc />
        public string ActivityFormId { get; set; }

        /// <inheritdoc />
        public ActivityOptions Options { get; } = new ActivityOptions();

        /// <inheritdoc />
        public MethodHandler MethodHandler { get; set; }

        /// <inheritdoc />
        public string FormTitle { get; set; }

        /// <inheritdoc />
        public int Position { get; set; }


        public ActivityFlowMock(WorkflowInformation workflowInformation,
            WorkflowCache workflowCache, string formTitle, string activityFormId, int position)
        {
            WorkflowInformation = workflowInformation;
            WorkflowCache = workflowCache;

            ActivityFormId = activityFormId.ToLowerInvariant();
            FormTitle = formTitle;
            MethodHandler = new MethodHandler(formTitle);
            Position = position;
           
            Options.From(workflowInformation.DefaultActivityOptions);
        }
    }
}