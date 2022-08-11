using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract;
using Nexus.Link.Components.WorkflowMgmt.Abstract.Services;
using Version = Nexus.Link.Components.WorkflowMgmt.Abstract.Entities.Version;

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
    public Task<IEnumerable<Version>> ReadChildrenAsync(string parentId, int limit = 2147483647, CancellationToken cancellationToken = default)
    {
        // TODO: How to do this?
        throw new NotImplementedException();
    }
}