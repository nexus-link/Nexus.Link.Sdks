using System;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;

namespace Nexus.Link.WorkflowEngine.Sdk
{
    /// <summary>
    /// A container for all workflow implementations for a specific workflow
    /// </summary>
    [Obsolete("Please use WorkflowContainer. Obsolete since 2022-02-10")]
    public abstract class WorkflowVersions : WorkflowContainer, IWorkflowVersions
    {
        /// <inheritdoc />
        protected WorkflowVersions(string capabilityName, string workflowTitle, string workflowId, IWorkflowEngineRequiredCapabilities workflowCapabilities) : base(capabilityName, workflowTitle, workflowId, workflowCapabilities)
        {
        }
    }
}