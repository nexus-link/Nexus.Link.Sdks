using System;
using System.Threading;
using System.Threading.Tasks;

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces
{
    public interface IActivityLoopUntilTrueBase : IActivity
    {
        bool? EndLoop { get; set; }

        T GetLoopArgument<T>(string name);
        void SetLoopArgument<T>(string name, T value);
    }

    public interface IActivityLoopUntilTrue : IActivityLoopUntilTrueBase
    {
        Task ExecuteAsync(Func<IActivityLoopUntilTrue, CancellationToken, Task> method, CancellationToken cancellationToken = default);
    }

    public interface IActivityLoopUntilTrue<TActivityReturns> : IActivityLoopUntilTrueBase
    {
        Task<TActivityReturns> ExecuteAsync(Func<IActivityLoopUntilTrue<TActivityReturns>, CancellationToken, Task<TActivityReturns>> method, CancellationToken cancellationToken = default);
    }
}