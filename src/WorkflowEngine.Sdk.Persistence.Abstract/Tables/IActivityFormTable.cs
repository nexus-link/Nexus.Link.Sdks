using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables
{
    public interface IActivityFormTable:
        ICreateWithSpecifiedIdAndReturn<ActivityFormRecordCreate, ActivityFormRecord, Guid>,
        IRead<ActivityFormRecord, Guid>,
        IUpdateAndReturn<ActivityFormRecord, Guid>
    {
        /// <summary>
        /// Search for all activity forms that belongs a a specific <paramref name="workflowFormId"/>.
        /// </summary>
        Task<IEnumerable<ActivityFormRecord>> SearchByWorkflowFormIdAsync(Guid workflowFormId, int limit = int.MaxValue, CancellationToken cancellationToken = default);
    }
}