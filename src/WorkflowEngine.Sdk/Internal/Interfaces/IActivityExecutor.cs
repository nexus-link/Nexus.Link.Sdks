using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;

internal interface IActivityExecutor
{
    public IInternalActivity Activity { get; }

    Task ExecuteWithoutReturnValueAsync(ActivityMethodAsync methodAsync, CancellationToken cancellationToken = default);

    Task<TMethodReturns> ExecuteWithReturnValueAsync<TMethodReturns>(ActivityMethodAsync<TMethodReturns> methodAsync, ActivityDefaultValueMethodAsync<TMethodReturns> getDefaultValueAsync,
        CancellationToken cancellationToken = default);
}