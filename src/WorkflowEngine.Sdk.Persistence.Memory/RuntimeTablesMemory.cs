﻿using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory.Tables;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory
{
    public class RuntimeTablesMemory : IRuntimeTables
    {
        public RuntimeTablesMemory()
        {
            WorkflowInstance = new WorkflowInstanceTableMemory();
            ActivityInstance = new ActivityInstanceTableMemory();
            Log = new LogTableMemory();
        }

        /// <inheritdoc />
        public IWorkflowInstanceTable WorkflowInstance { get; }

        /// <inheritdoc />
        public IActivityInstanceTable ActivityInstance { get; }

        /// <inheritdoc />
        public ILogTable Log { get; }
    }
}
