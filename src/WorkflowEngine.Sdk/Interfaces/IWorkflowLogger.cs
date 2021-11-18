using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Logging;

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces
{
    public interface IWorkflowLogger
    {
        /// <summary>
        /// Log one message and try to associate it to a WorkflowInstance and ActivityForm.
        /// </summary>
        /// <param name="severityLevel">The severity level for this log.</param>
        /// <param name="message">A string message with textual information.</param>
        /// <param name="data">Optional </param>
        /// <param name="cancellationToken"></param>
        Task LogAtLevelAsync(LogSeverityLevel severityLevel, string message, object data = null,
            CancellationToken cancellationToken = default);
    }
}