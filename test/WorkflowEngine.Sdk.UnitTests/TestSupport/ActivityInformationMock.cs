using System.Collections.Generic;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Support;

namespace WorkflowEngine.Sdk.UnitTests.TestSupport
{
    internal class ActivityInformationMock : IActivityInformation
    {
        public ActivityInformationMock(IWorkflowInformation workflowInformation)
        {
            Workflow = workflowInformation;
        }
        /// <inheritdoc />
        public IWorkflowInformation Workflow { get; } 

        /// <inheritdoc />
        public IInternalActivity Parent { get; set; }

        /// <inheritdoc />
        public IInternalActivity Previous { get; set; }

        /// <inheritdoc />
        public int Position { get; set; } = 1;

        /// <inheritdoc />
        public string FormId { get; set; } = "CA6994D0-2DE5-413F-9972-DDBBC2B2DE22";

        /// <inheritdoc />
        public string FormTitle { get; set; } = "Form title";

        /// <inheritdoc />
        public ActivityTypeEnum Type { get; set; } = ActivityTypeEnum.Action;

        /// <inheritdoc />
        public ActivityOptions Options { get; set; } = new ActivityOptions();

        /// <inheritdoc />
        public ICollection<LogCreate> Logs { get; set; } = new List<LogCreate>();

        /// <inheritdoc />
        public void DefineParameter<T>(string name)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public void SetParameter<T>(string name, T value)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public T GetArgument<T>(string parameterName)
        {
            throw new System.NotImplementedException();
        }
    }
}