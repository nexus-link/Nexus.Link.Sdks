using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces
{
    /// <summary>
    /// An activity of type <see cref="ActivityTypeEnum.Semaphore"/>.
    /// </summary>
    public interface IActivitySemaphore : IActivity
    {
        /// <summary>
        /// Raise a semaphore
        /// </summary>
        /// <param name="resourceIdentifier">A string that uniquely identifies the resource that needs to be protected.</param>
        /// <param name="expiresAfter">How long time can we hold the semaphore?</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The id for the semaphore; please use this id when you lower the semaphore.</returns>
        Task<string> RaiseAsync(string resourceIdentifier, TimeSpan expiresAfter, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lower a semaphore
        /// </summary>
        /// <param name="semaphoreId">The semaphore id that was returned by <see cref="RaiseAsync"/>.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The id for the semaphore; please use this id when you lower the semaphore.</returns>
        Task LowerAsync(string semaphoreId, CancellationToken cancellationToken = default);
    }
}