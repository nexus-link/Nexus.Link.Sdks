using System.Threading;
using System.Threading.Tasks;

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces
{
    public interface IActivityExceptionAlertHandler
    {
        public Task<bool> HandleActivityExceptionAlertAsync(ActivityExceptionAlert alert,
            CancellationToken cancellationToken = default);
    }
}