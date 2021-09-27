using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Web.Pipe;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Support;

namespace WorkflowEngine.Sdk.Outbound
{
    /// <summary>
    /// If the current execution is asynchronous, add a "Prefer: respond-async" header.
    /// </summary>
    public class PreferRespondAsync : DelegatingHandler
    {
        /// <summary>
        /// If the current execution is asynchronous, add a "Prefer: respond-async" header.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (AsyncWorkflowStatic.Context.ExecutionIsAsynchronous)
            {
                var headerAlreadyExists = request.Headers.TryGetValues(Constants.PreferHeaderName, out var preferHeader)
                                   && preferHeader.Contains(Constants.PreferRespondAsyncHeaderValue);
                if (!headerAlreadyExists)
                {
                    request.Headers.Add(Constants.PreferHeaderName, Constants.PreferRespondAsyncHeaderValue);
                }
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
