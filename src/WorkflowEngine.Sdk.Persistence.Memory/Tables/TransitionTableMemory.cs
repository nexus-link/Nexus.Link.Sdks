using System;
using Nexus.Link.Libraries.Crud.MemoryStorage;
using WorkflowEngine.Persistence.Abstract.Entities;
using WorkflowEngine.Persistence.Abstract.Tables;

namespace WorkflowEngine.Persistence.Memory.Tables
{
    public class TransitionTableMemory : ManyToOneMemory<TransitionRecordCreate, TransitionRecord, Guid>, ITransitionTable
    {
        /// <inheritdoc />
        public TransitionTableMemory() : base(item => item.WorkflowVersionId)
        {
        }
    }
}