using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.AsyncCaller.Sdk.Dispatcher.Models;
using Nexus.Link.Libraries.Core.Health.Model;
using RawRequest = Nexus.Link.AsyncCaller.Sdk.Data.Models.RawRequest;

namespace Nexus.Link.AsyncCaller.Sdk.RestClients.Facade
{
    public interface IAsyncCallsClient : IResourceHealth
    {
        [Obsolete("Please use PostRawAsync. Obsolete since 2020-09-10")]
        Task<string> PostAsync(Request request, CancellationToken cancellationToken = default);
        Task<string> PostRawAsync(RawRequest rawRequest, CancellationToken cancellationToken = default);
        Task ClearAsync(CancellationToken cancellationToken = default);
    }
}
