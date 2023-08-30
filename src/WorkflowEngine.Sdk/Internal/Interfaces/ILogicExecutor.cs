using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Activities;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;

internal interface ILogicExecutor
{
    public IInternalActivity Activity { get; }

    Task ExecuteWithoutReturnValueAsync(InternalActivityMethodAsync methodAsync, string methodName, CancellationToken cancellationToken = default);

    Task<TMethodReturns> ExecuteWithReturnValueAsync<TMethodReturns>(InternalActivityMethodAsync<TMethodReturns> methodAsync, string methodName,
        CancellationToken cancellationToken = default);
}