using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.WorkflowEngine.Sdk.WorkflowLogic;

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces
{
    public interface IActivityExecutor
    {
        Activity Activity { get; set; }

        Task ExecuteAsync(
            ActivityMethod method, 
            CancellationToken cancellationToken);
        Task<TMethodReturnType> ExecuteAsync<TMethodReturnType>(
            ActivityMethod<TMethodReturnType> method, 
            Func<CancellationToken, Task<TMethodReturnType>> getDefaultValueMethodAsync, 
            CancellationToken cancellationToken);
    }
}