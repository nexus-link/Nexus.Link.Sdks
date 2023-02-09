using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.Components.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Components.WorkflowMgmt.Abstract.Services;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;

namespace Nexus.Link.WorkflowEngine.Sdk.Services.Administration;

/// <inheritdoc />
public class FormOverviewService : IFormOverviewService
{
    private readonly IConfigurationTables _configurationTables;

    /// <summary>
    /// Constructor
    /// </summary>
    public FormOverviewService(IConfigurationTables configurationTables)
    {
        _configurationTables = configurationTables;
    }

    /// <inheritdoc />
    public async Task<IList<WorkflowFormOverview>> ReadByIntervalWithPagingAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken = default)
    {
        var result = await _configurationTables.WorkflowForm.ReadByIntervalWithPagingAsync(from, to, cancellationToken);
        return result.Select(x => new WorkflowFormOverview
        {
            Id = x.Id,
            CapabilityName = x.CapabilityName,
            Title = x.Title,
            Overview = x.Overview
        }).ToList();
    }
}