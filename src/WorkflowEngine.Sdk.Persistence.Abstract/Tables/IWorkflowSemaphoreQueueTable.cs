using System;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables
{
    /// <summary>
    /// A table for the <see cref="WorkflowSemaphoreRecord"/>
    /// </summary>
    public interface IWorkflowSemaphoreQueueTable :
        ICreateAndReturn<WorkflowSemaphoreQueueRecordCreate, WorkflowSemaphoreQueueRecord, Guid>,
        ISearch<WorkflowSemaphoreQueueRecord, Guid>,
        IRead<WorkflowSemaphoreQueueRecord, Guid>,
        IUpdate<WorkflowSemaphoreQueueRecord, Guid>,
        IUpdateAndReturn<WorkflowSemaphoreQueueRecord, Guid>,
        IDelete<Guid>
    {
    }
}