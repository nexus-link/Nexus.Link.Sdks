using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.Components.WorkflowMgmt.Abstract.Services;
using Nexus.Link.Libraries.Core.Assert;

namespace Nexus.Link.WorkflowEngine.Sdk.Services.Administration;

/// <inheritdoc />
public class FormService : IFormService
{
    private readonly IWorkflowConfigurationCapability _configurationCapability;

    /// <summary>
    /// Constructor
    /// </summary>
    public FormService(IWorkflowConfigurationCapability configurationCapability)
    {
        _configurationCapability = configurationCapability;
    }
    
    /// <inheritdoc />
    public async Task<WorkflowForm> ReadAsync(string id, CancellationToken cancellationToken = default)
    {
        InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));

        return await _configurationCapability.WorkflowForm.ReadAsync(id, cancellationToken);

    }
}