#if NETCOREAPP
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nexus.Link.Libraries.Web.Serialization;

namespace WorkflowEngine.Sdk.Inbound.RespondAsync
{
    /// <summary>
    /// Methods for deciding if a request should be responded to asynchronously and to actually execute a postponed request.
    /// </summary>
    public interface IRequestExecutor : IAlreadyRunningAsynchronously
    {
        /// <summary>
        /// This method is called when it is time to actually execute a request from the queue.
        /// </summary>
        /// <param name="requestData">The request that we should execute.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<ResponseData> ExecuteRequestAsync(RequestData requestData, CancellationToken cancellationToken = default);
    }

    public interface IAlreadyRunningAsynchronously
    {
        /// <summary>
        /// True if this request is marked as it is already running asynchronously.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        bool IsRunningAsynchronously(HttpRequest request);
    }
}
#endif