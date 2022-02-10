﻿using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract;
using Nexus.Link.Capabilities.WorkflowState.Abstract;

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces;

/// <summary>
/// The capabilities that are required by the workflow engine.
/// </summary>
public interface IWorkflowEngineRequiredCapabilities
{
    /// <summary>
    /// Information about the workflow configurations
    /// </summary>
    IWorkflowConfigurationCapability ConfigurationCapability { get; }

    /// <summary>
    /// Information about the state of workflow instances
    /// </summary>
    IWorkflowStateCapability StateCapability { get; }

    /// <summary>
    /// The async request management capability, used for re-entrance to the workflows.
    /// </summary>
    IAsyncRequestMgmtCapability RequestMgmtCapability{ get; }
}