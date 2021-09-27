using System;
using Nexus.Link.Libraries.Crud.MemoryStorage;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory.Tables
{
    public class TransitionTableMemory : ManyToOneMemory<TransitionRecordCreate, TransitionRecord, Guid>, ITransitionTable
    {
        /// <inheritdoc />
        public TransitionTableMemory() : base(item => item.WorkflowVersionId)
        {
        }
    }
}