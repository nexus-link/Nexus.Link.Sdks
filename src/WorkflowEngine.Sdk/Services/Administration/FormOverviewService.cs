using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Component.Services;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Entities;
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
    /// 
    public async Task<IList<WorkflowFormOverview>> ReadByIntervalWithPagingAsync(DateTimeOffset from, DateTimeOffset to, FormOverviewIncludeFilter filter, CancellationToken cancellationToken = default)
    {
        var result = await _configurationTables.WorkflowForm.ReadByIntervalWithPagingAsync(from, to, filter, cancellationToken);
        return result.Select(x => new WorkflowFormOverview
        {
            Id = x.Id,
            CapabilityName = x.CapabilityName,
            Title = x.Title,
            Version = x.Version,
            Overview = x.Overview
        }).ToList();
    }
}