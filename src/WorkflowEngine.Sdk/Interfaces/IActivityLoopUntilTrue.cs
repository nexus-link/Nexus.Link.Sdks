using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces
{
    /// <summary>
    /// An activity of type <see cref="ActivityTypeEnum.LoopUntilTrue"/>.
    /// </summary>
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