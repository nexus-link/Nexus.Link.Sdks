using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;

internal interface IActivityExecutor
{
    public IInternalActivity Activity { get; }

    Task ExecuteWithoutReturnValueAsync(InternalActivityMethodAsync methodAsync, CancellationToken cancellationToken = default);

    Task<TMethodReturns> ExecuteWithReturnValueAsync<TMethodReturns>(InternalActivityMethodAsync<TMethodReturns> methodAsync, ActivityDefaultValueMethodAsync<TMethodReturns> getDefaultValueAsync,
        CancellationToken cancellationToken = default);
}