using System.Threading.Tasks;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract
{
    /// <summary>
    /// All tables that have records that are created and changed during runtime
    /// </summary>
    public interface IRuntimeTables : IDeleteAll
    {
        /// <summary>
        /// Service for <see cref="IWorkflowInstanceTable"/>.
        /// </summary>
        IWorkflowInstanceTable WorkflowInstance { get; }

        /// <summary>
        /// Service for <see cref="IActivityInstanceTable"/>.
        /// </summary>
        IActivityInstanceTable ActivityInstance { get; }

        /// <summary>
        /// Service for <see cref="ILogTable"/>.
        /// </summary>
        ILogTable Log { get; }

        /// <summary>
        /// Service for <see cref="IWorkflowSemaphoreTable"/>.
        /// </summary>
        IWorkflowSemaphoreTable WorkflowSemaphore { get; }

        /// <summary>
        /// Service for <see cref="IWorkflowSemaphoreQueueTable"/>.
        /// </summary>
        IWorkflowSemaphoreQueueTable WorkflowSemaphoreQueue { get; }
    }
}