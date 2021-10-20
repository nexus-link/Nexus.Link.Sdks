using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.WorkflowEngine.Sdk.Model;

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces
{
    public interface IActivityExceptionAlertHandler
    {
        public Task<bool> HandleActivityExceptionAlert(ActivityExceptionAlert alert,
            CancellationToken cancellationToken = default);
    }
}