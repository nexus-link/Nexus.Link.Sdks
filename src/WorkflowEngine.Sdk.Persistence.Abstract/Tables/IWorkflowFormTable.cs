using System;
using Nexus.Link.Libraries.Crud.Interfaces;
using WorkflowEngine.Persistence.Abstract.Entities;

namespace WorkflowEngine.Persistence.Abstract.Tables
{
    public interface IWorkflowFormTable: ICreateWithSpecifiedId<WorkflowFormRecordCreate, WorkflowFormRecord, Guid>, IRead<WorkflowFormRecord, Guid>, IUpdate<WorkflowFormRecord, Guid>, ISearch<WorkflowFormRecord, Guid>
    {
    }
}