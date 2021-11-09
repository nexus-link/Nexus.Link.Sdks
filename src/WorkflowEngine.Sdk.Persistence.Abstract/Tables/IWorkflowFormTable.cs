using System;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables
{
    public interface IWorkflowFormTable: ICreateWithSpecifiedId<WorkflowFormRecordCreate, WorkflowFormRecord, Guid>, IRead<WorkflowFormRecord, Guid>, IUpdate<WorkflowFormRecord, Guid>, ISearch<WorkflowFormRecord, Guid>
    {
    }
}