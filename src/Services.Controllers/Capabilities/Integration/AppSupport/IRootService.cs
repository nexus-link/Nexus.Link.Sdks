using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Nexus.Link.Services.Controllers.Capabilities.Integration.AppSupport
{
    /// <summary>
    /// Our default service for an API root
    /// </summary>
    public interface IRootService
    {
        /// <summary>
        /// The method that should be called when someone makes a GET request on the root of the service.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<ContentResult> Welcome(CancellationToken cancellationToken = default);
    }
}