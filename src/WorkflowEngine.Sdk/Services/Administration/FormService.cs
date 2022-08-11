using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract;
using Nexus.Link.Components.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Components.WorkflowMgmt.Abstract.Services;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Storage.Model;

namespace Nexus.Link.WorkflowEngine.Sdk.Services.Administration;

/// <inheritdoc />
public class FormService : IFormService
{
    private readonly IWorkflowConfigurationCapability _configurationCapability;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="configurationCapability"></param>
    public FormService(IWorkflowConfigurationCapability configurationCapability)
    {
        _configurationCapability = configurationCapability;
    }

    /// <inheritdoc />
    public async Task<Form> ReadAsync(string id, CancellationToken cancellationToken = default)
    {
        InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));

        var form = await _configurationCapability.WorkflowForm.ReadAsync(id, cancellationToken);
        return new Form
        {
            Id = form.Id,
            Title = form.Title
        };
    }

    /// <inheritdoc />
    public async Task<PageEnvelope<Form>> ReadAllWithPagingAsync(int offset, int? limit = null, CancellationToken cancellationToken = default)
    {
        var result = await _configurationCapability.WorkflowForm.ReadAllWithPagingAsync(offset, limit, cancellationToken);
        return new PageEnvelope<Form>
        {
            PageInfo = result.PageInfo,
            Data = result.Data.Select(x => new Form
            {
                Id = x.Id,
                Title = x.Title
            })
        };
    }
}