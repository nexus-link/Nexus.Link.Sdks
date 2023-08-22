using System;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;

namespace Nexus.Link.WorkflowEngine.Sdk.Exceptions
{
    internal class WorkflowEngineInternalErrorException : WorkflowFailedException
    {
        /// <summary>
        /// Use this exception to notify the workflow implementor that the workflow engine had an internal error outside any specific activity.
        /// </summary>
        public WorkflowEngineInternalErrorException(IWorkflowInformation workflowInformation, Exception e) : base(
            ActivityExceptionCategoryEnum.WorkflowCapabilityError,
            $"Internal error in the workflow engine when executing workflow {workflowInformation?.ToLogString()}: {e?.ToLogString()}",
            "The workflow engine had an internal error.")
        {
        }

        /// <summary>
        /// Use this exception to notify the workflow implementor that the workflow engine had an internal error outside any specific activity.
        /// </summary>
        public WorkflowEngineInternalErrorException(IActivityInformation activityInformation, Exception e) : base(
            ActivityExceptionCategoryEnum.WorkflowCapabilityError,
            $"Internal error in the workflow engine when executing workflow {activityInformation?.Workflow?.ToLogString()}. Related to activity {activityInformation?.ToLogString()}: {e?.ToLogString()}",
            "The workflow engine had an internal error.")
        {
        }
    }
}