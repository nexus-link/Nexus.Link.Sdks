using System;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables
{
    public interface IMethodParameterTable: 
        ICreateDependentWithSpecifiedId<MethodParameterRecordCreate, MethodParameterRecord, Guid, string>, 
        IReadDependent<MethodParameterRecord, Guid, string>,
        IReadChildrenWithPaging<MethodParameterRecord, Guid>
    {
    }
}