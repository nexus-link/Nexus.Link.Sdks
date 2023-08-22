using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables
{
    public interface IWorkflowInstanceTable: 
        ICreateWithSpecifiedIdAndReturn<WorkflowInstanceRecordCreate, WorkflowInstanceRecord, Guid>,
        IRead<WorkflowInstanceRecord, Guid>, 
        IUpdateAndReturn<WorkflowInstanceRecord, Guid>,
        ISearch<WorkflowInstanceRecord, Guid>,
        IDistributedLock<Guid>
    {
        /// <summary>
        /// Search/filter instances
        /// </summary>
        Task<PageEnvelope<WorkflowInstanceRecord>> SearchAsync(WorkflowInstanceSearchDetails instanceSearchDetails, int offset = 0, int? limit = null, CancellationToken cancellationToken = default);
    }
}