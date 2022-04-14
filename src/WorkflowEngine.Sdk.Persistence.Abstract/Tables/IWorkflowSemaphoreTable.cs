using System;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables
{
    /// <summary>
    /// A table for the <see cref="WorkflowSemaphoreRecord"/>
    /// </summary>
    public interface IWorkflowSemaphoreTable: 
        ICreate<WorkflowSemaphoreRecordCreate, WorkflowSemaphoreRecord, Guid>,
        IRead<WorkflowSemaphoreRecord, Guid>,
        IUpdate<WorkflowSemaphoreRecord, Guid>,
        ISearch<WorkflowSemaphoreRecord, Guid>,
        ITransactionLock<WorkflowSemaphoreRecord, Guid>,
        IDelete<Guid>
    {
    }
}