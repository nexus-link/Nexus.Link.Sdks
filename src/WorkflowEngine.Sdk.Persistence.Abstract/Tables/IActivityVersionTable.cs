using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables
{
    public interface IActivityVersionTable: 
        ICreateWithSpecifiedIdAndReturn<ActivityVersionRecordCreate, ActivityVersionRecord, Guid>, 
        IRead<ActivityVersionRecord, Guid>, 
        IUpdateAndReturn<ActivityVersionRecord, Guid>
    {

        /// <summary>
        /// Search for all activity forms that belongs a a specific <paramref name="workflowVersionId"/>.
        /// </summary>
        Task<IEnumerable<ActivityVersionRecord>> SearchByWorkflowVersionIdAsync(Guid workflowVersionId, int limit = int.MaxValue, CancellationToken cancellationToken = default);
    }
}