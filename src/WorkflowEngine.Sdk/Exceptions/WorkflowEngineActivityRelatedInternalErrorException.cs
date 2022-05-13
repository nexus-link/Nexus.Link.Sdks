using System;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;

namespace Nexus.Link.WorkflowEngine.Sdk.Exceptions
{
    internal class WorkflowEngineActivityRelatedInternalErrorException : ActivityFailedException
    {
        /// <summary>
        /// Use this exception to notify the workflow implementor that the workflow engine had an internal error for a specific activity
        /// </summary>
        public WorkflowEngineActivityRelatedInternalErrorException(IInternalActivity activity, Exception e) : base(
            ActivityExceptionCategoryEnum.WorkflowCapabilityError, 
            $"Internal error in the workflow engine when executing workflow {activity?.ActivityInformation?.Workflow?.ToLogString()}, activity {activity?.ToLogString()}: {e?.ToLogString()}", 
            "The workflow engine had an internal error.")
        {
        }
    }
}