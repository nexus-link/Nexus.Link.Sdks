using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract.Services;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Web.RestClientHelper;

namespace Nexus.Link.AsyncManager.Sdk.RestClients
{
    /// <inheritdoc />
    public class ExecutionRestClient : IExecutionService
    {
        private readonly IHttpSender _httpSender;

        /// <summary>
        /// Constructor
        /// </summary>
        public ExecutionRestClient(IHttpSender httpSender)
        {
            _httpSender = httpSender;
        }

        /// <inheritdoc />
        public async Task ReadyForExecutionAsync(string executionId, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(executionId, nameof(executionId));

            var relativeUrl = $"Executions/{WebUtility.UrlEncode(executionId)}/ReadyForExecution";
            await _httpSender.SendRequestAsync<HttpResponseMessage>(HttpMethod.Post, relativeUrl, cancellationToken: cancellationToken);
        }
    }
}