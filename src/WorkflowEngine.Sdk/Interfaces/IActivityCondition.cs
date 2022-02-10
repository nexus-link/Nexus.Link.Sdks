using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces
{
    /// <summary>
    /// An activity of type <see cref="ActivityTypeEnum.Condition"/>.
    /// </summary>
    public interface IActivityCondition<TActivityReturns> : IActivity<TActivityReturns>
    {
        /// <summary>
        /// The logic to calculate the condition value
        /// </summary>
        Task<TActivityReturns> ExecuteAsync(
            Func<IActivityCondition<TActivityReturns>, CancellationToken, Task<TActivityReturns>> conditionMethodAsync,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// The logic to calculate the condition value
        /// </summary>
        Task<TActivityReturns> ExecuteAsync(
            Func<IActivityCondition<TActivityReturns>, TActivityReturns> conditionMethod,
            CancellationToken cancellationToken = default);
    }
}