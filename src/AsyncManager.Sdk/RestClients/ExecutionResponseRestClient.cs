using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract.Services;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Web.RestClientHelper;
using Nexus.Link.Libraries.Web.Serialization;

namespace Nexus.Link.AsyncManager.Sdk.RestClients
{

    /// <inheritdoc />
    public class ExecutionResponseRestClient : IExecutionResponseService
    {
        private readonly IHttpSender _httpSender;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="httpSender"></param>
        public ExecutionResponseRestClient(IHttpSender httpSender)
        {
            _httpSender = httpSender;
        }

        /// <inheritdoc />
        public async Task CreateAsync(string executionId, ResponseData responseData, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(executionId, nameof(executionId));
            InternalContract.RequireNotNull(responseData, nameof(responseData));

            var relativeUrl = $"Executions/{WebUtility.UrlEncode(executionId)}/Response";
            await _httpSender.SendRequestAsync<HttpResponseMessage, ResponseData>(HttpMethod.Post, relativeUrl, responseData, cancellationToken: cancellationToken);
        }
    }
}
