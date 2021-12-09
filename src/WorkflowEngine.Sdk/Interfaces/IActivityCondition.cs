using System;
using System.Threading;
using System.Threading.Tasks;

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces
{
    public interface IActivityCondition<TActivityReturns> : IActivity<TActivityReturns>
    {
        Task<TActivityReturns> ExecuteAsync(
            Func<IActivityCondition<TActivityReturns>, CancellationToken, Task<TActivityReturns>> conditionMethodAsync, 
            CancellationToken cancellationToken = default);
    }
}