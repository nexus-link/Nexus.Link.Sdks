using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract.Services;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Web.RestClientHelper;

namespace Nexus.Link.AsyncManager.Sdk.RestClients
{
    /// <inheritdoc />
    public class ExecutionRestClient : RestClient, IExecutionService
    {
        private readonly IHttpSender _httpSender;

        /// <summary>
        /// Constructor
        /// </summary>
        public ExecutionRestClient(IHttpSender httpSender)
        : base(httpSender)
        {
            _httpSender = httpSender;
        }

        /// <inheritdoc />
        public Task ReadyForExecutionAsync(string executionId, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(executionId, nameof(executionId));

            var relativeUrl = $"Executions/{WebUtility.UrlEncode(executionId)}/ReadyForExecution";
            return PostNoResponseContentAsync(relativeUrl, null, cancellationToken);
        }
    }
}