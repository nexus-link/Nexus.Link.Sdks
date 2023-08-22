using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Component.Services;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Entities;

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

    /// <inheritdoc />
    public async Task<PageEnvelope<WorkflowForm>> ReadAllWithPagingAsync(int offset, int? limit = null, CancellationToken cancellationToken = default)
    {
        InternalContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
        if (limit.HasValue) InternalContract.RequireGreaterThanOrEqualTo(1, limit.Value, nameof(limit));

        return await _configurationCapability.WorkflowForm.ReadAllWithPagingAsync(offset, limit, cancellationToken);
    }
}