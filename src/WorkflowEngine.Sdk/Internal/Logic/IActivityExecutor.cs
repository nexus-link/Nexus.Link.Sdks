using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;

internal interface IActivityExecutor
{
    public Activity Activity { get; }

    Task ExecuteWithoutReturnValueAsync(ActivityMethod method, CancellationToken cancellationToken = default);

    Task<TMethodReturnType> ExecuteWithReturnValueAsync<TMethodReturnType>(
        ActivityMethod<TMethodReturnType> method,
        Func<CancellationToken, Task<TMethodReturnType>> getDefaultValueMethodAsync,
        CancellationToken cancellationToken = default);
}