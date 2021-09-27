using System;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables
{
    public interface ITransitionTable: ICreateChild<TransitionRecordCreate, TransitionRecord, Guid>, IReadChildrenWithPaging<TransitionRecord, Guid>, ISearchChildren<TransitionRecord, Guid>
    {
    }
}