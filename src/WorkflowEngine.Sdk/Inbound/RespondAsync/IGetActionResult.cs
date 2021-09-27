#if NETCOREAPP
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace WorkflowEngine.Sdk.Inbound.RespondAsync
{
    /// <summary>
    /// Method to get an action result
    /// </summary>
    public interface IGetActionResult
    {
        /// <summary>
        /// Get the result as <see cref="IActionResult"/>.
        /// </summary>
        /// <param name="requestId">The id of the request that we want an <see cref="IActionResult"/> for.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IActionResult> GetActionResultAsync(Guid requestId, CancellationToken cancellationToken = default);

        string GetResponseUrl(Guid requestId);
    }
}
#endif