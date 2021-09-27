using System;
using Nexus.Link.Libraries.Crud.Interfaces;
using WorkflowEngine.Persistence.Abstract.Entities;

namespace WorkflowEngine.Persistence.Abstract.Tables
{
    public interface IMethodParameterTable: 
        ICreateDependentWithSpecifiedId<MethodParameterRecordCreate, MethodParameterRecord, Guid, string>, 
        IReadDependent<MethodParameterRecord, Guid, string>,
        IReadChildrenWithPaging<MethodParameterRecord, Guid>
    {
    }
}