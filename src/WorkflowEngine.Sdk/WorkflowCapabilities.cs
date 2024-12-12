using System;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
using Nexus.Link.Libraries.Core.EntityAttributes;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Execution;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;
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
    [Obsolete("Please use the constructor with WorkflowCapabilities(IConfigurationTables, IRuntimeTables, IStorage, IAsyncRequestMgmtCapability, WorflowOptions). Obsolete since 2022-02-10.")]
    public WorkflowCapabilities(IWorkflowConfigurationCapability configurationCapability, IWorkflowStateCapability stateCapability, IAsyncRequestMgmtCapability requestMgmtCapability)
    {
        ConfigurationCapability = configurationCapability;
        StateCapability = stateCapability;
        RequestMgmtCapability = requestMgmtCapability;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public WorkflowCapabilities(IConfigurationTables configurationTables, IRuntimeTables runtimeTables,
        IAsyncRequestMgmtCapability requestMgmtCapability, IWorkflowEngineStorage storage, WorkflowOptions workflowOptions = null)
    {
        ConfigurationCapability = new WorkflowConfigurationCapability(configurationTables);
        StateCapability = new WorkflowStateCapability(configurationTables, runtimeTables, storage, requestMgmtCapability, workflowOptions);
        RequestMgmtCapability = requestMgmtCapability;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public WorkflowCapabilities(IConfigurationTables configurationTables, IRuntimeTables runtimeTables, IAsyncRequestMgmtCapability requestMgmtCapability, WorkflowOptions workflowOptions = null)
        : this(configurationTables, runtimeTables, requestMgmtCapability, new NoWorkflowEngineStorage(), workflowOptions)
    {
    }
}