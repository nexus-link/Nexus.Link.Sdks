using System;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables
{
    public interface IWorkflowInstanceTable: 
        ICreateWithSpecifiedIdAndReturn<WorkflowInstanceRecordCreate, WorkflowInstanceRecord, Guid>,
        IRead<WorkflowInstanceRecord, Guid>, 
        IUpdateAndReturn<WorkflowInstanceRecord, Guid>,
        IDistributedLock<Guid>
    {
    }
}