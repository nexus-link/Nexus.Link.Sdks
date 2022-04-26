using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces
{
    /// <summary>
    /// An activity of type <see cref="ActivityTypeEnum.Action"/>.
    /// </summary>
    public interface IActivityAction : IActivity
    {
        Task ExecuteAsync(Func<IActivityAction, CancellationToken, Task> method, CancellationToken cancellationToken = default);
    }
    /// <summary>
    /// An activity of type <see cref="ActivityTypeEnum.Action"/>.
    /// </summary>
    public interface IActivityAction<TActivityReturns> : IActivity
    {
        Task<TActivityReturns> ExecuteAsync(
            Func<IActivityAction<TActivityReturns>, CancellationToken, Task<TActivityReturns>> method,
            CancellationToken cancellationToken = default);
    }
}