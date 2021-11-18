using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Logging;

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces
{
    public interface IWorkflowLogger
    {
        Task LogAtLevelAsync(LogSeverityLevel severityLevel, string message, object data,
            CancellationToken cancellationToken);
    }
}