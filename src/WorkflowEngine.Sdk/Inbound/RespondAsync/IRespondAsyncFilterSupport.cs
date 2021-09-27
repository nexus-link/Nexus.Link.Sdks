#if NETCOREAPP
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Nexus.Link.WorkflowEngine.Sdk.Inbound.RespondAsync
{
    /// <summary>
    /// The main driver for respond async.
    /// </summary>
    public interface IRespondAsyncFilterSupport: IGetActionResult, IAlreadyRunningAsynchronously, IExecuteAsync
    {
        /// <summary>
        /// Enqueue one request for eventual execution.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>A newly created request id or null.</returns>
        Task<Guid?> EnqueueAsync(HttpRequest request, CancellationToken cancellationToken = default);
    }
}
#endif