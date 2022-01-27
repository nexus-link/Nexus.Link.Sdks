using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Model;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Support;
using Nexus.Link.WorkflowEngine.Sdk.Support;

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

            ActivityFormId = activityFormId.ToGuidString();
            FormTitle = formTitle;
            MethodHandler = new MethodHandler(formTitle);
            Position = position;
           
            Options.From(workflowInformation.DefaultActivityOptions);
        }
    }
}