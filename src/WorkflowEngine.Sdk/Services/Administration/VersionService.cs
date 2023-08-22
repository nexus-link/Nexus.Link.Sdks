using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Component.Services;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Services.Administration;

/// <inheritdoc />
public class VersionService : IVersionService
{
    private readonly IWorkflowConfigurationCapability _configurationCapability;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="configurationCapability"></param>
    public VersionService(IWorkflowConfigurationCapability configurationCapability)
    {
        _configurationCapability = configurationCapability;
    }

    /// <inheritdoc />
    public Task<IEnumerable<WorkflowVersion>> ReadChildrenAsync(string parentId, int limit = 2147483647, CancellationToken cancellationToken = default)
    {
        // TODO: How to do this?
        //_configurationCapability.WorkflowVersion.
        throw new NotImplementedException();
    }
}