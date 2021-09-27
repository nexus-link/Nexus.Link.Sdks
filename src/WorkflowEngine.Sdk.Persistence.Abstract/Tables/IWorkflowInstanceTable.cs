using System;
using Nexus.Link.Libraries.Crud.Interfaces;
using WorkflowEngine.Persistence.Abstract.Entities;

namespace WorkflowEngine.Persistence.Abstract.Tables
{
    public interface IWorkflowInstanceTable: ICreateChildWithSpecifiedId<WorkflowInstanceRecordCreate, WorkflowInstanceRecord, Guid>, IRead<WorkflowInstanceRecord, Guid>
    {
    }
}