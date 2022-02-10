using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowState.Abstract;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Support;

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