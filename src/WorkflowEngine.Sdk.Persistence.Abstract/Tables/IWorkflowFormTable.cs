using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Component.Services;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables
{
    /// <inheritdoc />
    public interface IWorkflowFormTable:
        ICreateWithSpecifiedId<WorkflowFormRecordCreate, WorkflowFormRecord, Guid>,
        IRead<WorkflowFormRecord, Guid>, 
        IUpdateAndReturn<WorkflowFormRecord, Guid>,
        IReadAllWithPaging<WorkflowFormRecord, Guid>
    {
        /// <summary>
        /// Find a unique record with the specified <paramref name="capabilityName"/> and <paramref name="title"/> or null.
        /// </summary>
        Task<WorkflowFormRecord> FindByCapabilityNameAndTitleAsync(string capabilityName, string title, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get a list of forms with instance counts per state for the given interval
        /// </summary>
        Task<IList<WorkflowFormRecordOverview>> ReadByIntervalWithPagingAsync(DateTimeOffset instancesFrom, DateTimeOffset instancesTo, FormOverviewIncludeFilter filter, CancellationToken cancellationToken = default);
    }
}
