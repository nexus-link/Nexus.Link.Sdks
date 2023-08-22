using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Abstract.Component.Services;

/// <summary>
/// Management methods for a workflow form overview
/// </summary>
public interface IFormOverviewService
{
    /// <summary>
    /// Get a list of forms with instance counts per state for the given interval
    /// </summary>
    Task<IList<WorkflowFormOverview>> ReadByIntervalWithPagingAsync(DateTimeOffset instancesFrom, DateTimeOffset instancesTo, FormOverviewIncludeFilter filter , CancellationToken cancellationToken = default);
}

/// <summary>
/// Form overview filter
/// </summary>
public enum FormOverviewIncludeFilter
{
    /// <summary>
    /// Show all workflow
    /// </summary>
    All = 0,
    /// <summary>
    /// Exculde workflow with instance count zero
    /// </summary>
    ExcludeZeroInstanceCount = 1,
    /// <summary>
    ///  Show only the latest version workflow
    /// </summary>
    LatestVersion = 2
}