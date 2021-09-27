using System;
using Nexus.Link.Libraries.Crud.Interfaces;
using WorkflowEngine.Persistence.Abstract.Entities;

namespace WorkflowEngine.Persistence.Abstract.Tables
{
    public interface ITransitionTable: ICreateChild<TransitionRecordCreate, TransitionRecord, Guid>, IReadChildrenWithPaging<TransitionRecord, Guid>, ISearchChildren<TransitionRecord, Guid>
    {
    }
}