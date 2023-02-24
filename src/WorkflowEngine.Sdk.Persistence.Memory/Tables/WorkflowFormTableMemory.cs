using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Components.WorkflowMgmt.Abstract.Services;
using Nexus.Link.Libraries.Crud.MemoryStorage;
using Nexus.Link.Libraries.Crud.Model;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory.Tables
{
    public class WorkflowFormTableMemory : CrudMemory<WorkflowFormRecordCreate, WorkflowFormRecord, Guid>, IWorkflowFormTable
    {
        public WorkflowFormTableMemory()
        {
            UniqueConstraintMethods += item => new { item.CapabilityName, item.Title };
        }

        public Task<WorkflowFormRecord> FindByCapabilityNameAndTitleAsync(string capabilityName, string title,
            CancellationToken cancellationToken = default)
        {
            var search = new WorkflowFormRecordCreate
            {
                CapabilityName = capabilityName,
                Title = title
            };
            return FindUniqueAsync(new SearchDetails<WorkflowFormRecord>(search), cancellationToken);
        }

        public Task<IList<WorkflowFormRecordOverview>> ReadByIntervalWithPagingAsync(DateTimeOffset instancesFrom, DateTimeOffset instancesTo, FormOverviewIncludeFilter filter, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}