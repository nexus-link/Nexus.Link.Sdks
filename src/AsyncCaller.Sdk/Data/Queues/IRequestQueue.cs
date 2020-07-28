using System;
using System.Threading.Tasks;
using Xlent.Lever.AsyncCaller.Data.Models;
using Xlent.Lever.AsyncCaller.Storage.Queue;
using Nexus.Link.Libraries.Core.Health.Model;

namespace Xlent.Lever.AsyncCaller.Data.Queues
{
    public interface IRequestQueue : IResourceHealth, IResourceHealth2
    {
        IQueue GetQueue();
        Task<string> EnqueueAsync(RequestEnvelope requestEnvelope, TimeSpan? timeSpanToWait = null);
        Task RequeueAsync(RequestEnvelope requestEnvelope, DateTimeOffset? latestAttemptAt = null);
        Task ClearAsync();
    }
}
