using System;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables
{
    public interface ITransitionTable: ICreate<TransitionRecordCreate, TransitionRecord, Guid>, IReadChildrenWithPaging<TransitionRecord, Guid>, IRead<TransitionRecord, Guid>, ISearch<TransitionRecord, Guid>
    {
    }
}