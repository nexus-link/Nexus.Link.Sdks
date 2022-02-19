﻿using System;
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
        /// <param name="expiresAfter">How long time can we hold the semaphore?</param>
        /// <param name="cancellationToken"></param>
        Task RaiseAsync(TimeSpan expiresAfter, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lower a semaphore
        /// </summary>
        /// <param name="cancellationToken"></param>
        Task LowerAsync(CancellationToken cancellationToken = default);
    }
}