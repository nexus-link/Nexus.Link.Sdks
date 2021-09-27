using System;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables
{
    public interface IWorkflowVersionTable: 
        ICreateDependentWithSpecifiedId<WorkflowVersionRecordCreate, WorkflowVersionRecord, Guid, int>, 
        IReadDependent<WorkflowVersionRecord, Guid, int>, 
        IUpdateDependent<WorkflowVersionRecord, Guid, int>
    {
    }
}