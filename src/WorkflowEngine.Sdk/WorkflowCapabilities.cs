using System;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract;
using Nexus.Link.Capabilities.WorkflowState.Abstract;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Services;
using Nexus.Link.Libraries.Core.EntityAttributes;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.Services;

namespace Nexus.Link.WorkflowEngine.Sdk;

/// <inheritdoc cref="IWorkflowEngineRequiredCapabilities" />
public class WorkflowCapabilities : IWorkflowEngineRequiredCapabilities
{
    /// <inheritdoc />
    [Validation.NotNull]
    public IWorkflowConfigurationCapability ConfigurationCapability { get; set; }

    /// <inheritdoc />
    [Validation.NotNull]
    public IWorkflowStateCapability StateCapability { get; set; }

    /// <inheritdoc />
    [Validation.NotNull]
    public IAsyncRequestMgmtCapability RequestMgmtCapability { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    [Obsolete("Please use the constructor with WorkflowCapabilities(IConfigurationTables, IRuntimeTables, IAsyncRequestMgmtCapability). Obsolete since 2022-02-10.")]
    public WorkflowCapabilities(IWorkflowConfigurationCapability configurationCapability, IWorkflowStateCapability stateCapability, IAsyncRequestMgmtCapability requestMgmtCapability)
    {
        ConfigurationCapability = configurationCapability;
        StateCapability = stateCapability;
        RequestMgmtCapability = requestMgmtCapability;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// TODO: Testa i examples om det är brytande med WorkflowOptions i konstruktorn
    public WorkflowCapabilities(IConfigurationTables configurationTables, IRuntimeTables runtimeTables, IAsyncRequestMgmtCapability requestMgmtCapability, WorkflowOptions workflowOptions = null)
    {
        ConfigurationCapability = new WorkflowConfigurationCapability(configurationTables);
        StateCapability = new WorkflowStateCapability(configurationTables, runtimeTables, requestMgmtCapability, workflowOptions);
        RequestMgmtCapability = requestMgmtCapability;
    }
}