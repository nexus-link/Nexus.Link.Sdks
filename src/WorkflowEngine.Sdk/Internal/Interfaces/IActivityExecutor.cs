using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Activities;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;

internal interface IActivityExecutor
{
    public IInternalActivity Activity { get; }

    Task ExecuteWithoutReturnValueAsync(InternalActivityMethodAsync methodAsync, CancellationToken cancellationToken = default);

    Task<TActivityReturns> ExecuteWithReturnValueAsync<TActivityReturns>(InternalActivityMethodAsync<TActivityReturns> methodAsync, ActivityDefaultValueMethodAsync<TActivityReturns> getDefaultValueAsync,
        CancellationToken cancellationToken = default);

    Task DoExtraAdminAsync(Func<CancellationToken, Task> finishAction, CancellationToken cancellationToken);
}