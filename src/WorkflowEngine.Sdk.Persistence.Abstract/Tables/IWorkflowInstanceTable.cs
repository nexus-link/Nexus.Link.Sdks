using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables
{
    /// <summary>
    /// Services for dealing with a workflow instance.
    /// </summary>
    public interface IWorkflowInstanceTable: 
        ICreateWithSpecifiedIdAndReturn<WorkflowInstanceRecordCreate, WorkflowInstanceRecord, Guid>,
        IRead<WorkflowInstanceRecord, Guid>, 
        IUpdateAndReturn<WorkflowInstanceRecord, Guid>,
        ISearch<WorkflowInstanceRecord, Guid>,
        IDistributedLock<Guid>
    {
        /// <summary>
        /// Read the <see cref="WorkflowInstanceRecord" /> with the specified execution id.
        /// </summary>
        /// <param name="executionId">The execution id to search for.</param>
        /// <param name="cancellationToken"></param>
        Task<WorkflowInstanceRecord> ReadByExecutionIdAsync(string executionId, CancellationToken cancellationToken = default);
    }
}