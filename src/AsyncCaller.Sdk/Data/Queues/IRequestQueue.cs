﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.AsyncCaller.Sdk.Data.Models;
using Nexus.Link.AsyncCaller.Sdk.Storage.Queue;
using Nexus.Link.Libraries.Core.Health.Model;

namespace Nexus.Link.AsyncCaller.Sdk.Data.Queues
{
    public interface IRequestQueue : IResourceHealth, IResourceHealth2
    {
        IQueue GetQueue();
        Task<string> EnqueueAsync(RawRequestEnvelope rawRequestEnvelope, TimeSpan? timeSpanToWait = null, CancellationToken cancellationToken = default);
        Task RequeueAsync(RawRequestEnvelope rawRequestEnvelope, DateTimeOffset? latestAttemptAt = null, CancellationToken cancellationToken = default);
        Task ClearAsync(CancellationToken cancellationToken = default);
    }
}
