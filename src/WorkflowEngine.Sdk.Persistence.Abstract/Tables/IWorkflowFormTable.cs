using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables
{
    public interface IWorkflowFormTable:
        ICreateWithSpecifiedId<WorkflowFormRecordCreate, WorkflowFormRecord, Guid>,
        IRead<WorkflowFormRecord, Guid>, 
        IUpdateAndReturn<WorkflowFormRecord, Guid>
    {
        /// <summary>
        /// Find a unique record with the specified <paramref name="capabilityName"/> and <paramref name="title"/> or null.
        /// </summary>
        Task<WorkflowFormRecord> FindByCapabilityNameAndTitleAsync(string capabilityName, string title, CancellationToken cancellationToken = default);
    }
}