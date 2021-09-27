using System;
using Nexus.Link.Libraries.Crud.Interfaces;
using WorkflowEngine.Persistence.Abstract.Entities;

namespace WorkflowEngine.Persistence.Abstract.Tables
{
    public interface IWorkflowVersionTable: 
        ICreateDependentWithSpecifiedId<WorkflowVersionRecordCreate, WorkflowVersionRecord, Guid, int>, 
        IReadDependent<WorkflowVersionRecord, Guid, int>, 
        IUpdateDependent<WorkflowVersionRecord, Guid, int>
    {
    }
}