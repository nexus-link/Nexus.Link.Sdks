using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Interfaces;
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
        Task<PageEnvelope<WorkflowInstanceRecord>> SearchAsync(WorkflowSearchDetails searchDetails, int offset = 0, int limit = 50, CancellationToken cancellationToken = default);
    }
}