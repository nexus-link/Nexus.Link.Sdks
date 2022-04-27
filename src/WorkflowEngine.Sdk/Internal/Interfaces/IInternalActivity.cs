using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;

internal interface IInternalActivity : IInternalActivityBase, IActivity
{
    ActivityInstance Instance { get; }
    ActivityVersion Version { get; }
    void MaybePurgeLogs();
    Task SafeAlertExceptionAsync(CancellationToken cancellationToken);
}