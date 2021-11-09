using System;
using System.Threading;
using System.Threading.Tasks;

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces
{
    public interface IActivityAction : IActivity
    {
        Task ExecuteAsync(Func<IActivityAction, CancellationToken, Task> method, CancellationToken cancellationToken = default);
    }

    public interface IActivityAction<TActivityReturns> : IActivity
    {
        Task<TActivityReturns> ExecuteAsync(
            Func<IActivityAction<TActivityReturns>, CancellationToken, Task<TActivityReturns>> method,
            CancellationToken cancellationToken = default);
    }
}