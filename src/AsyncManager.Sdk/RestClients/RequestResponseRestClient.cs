using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract.Entities;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract.Services;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Web.RestClientHelper;

namespace Nexus.Link.AsyncManager.Sdk.RestClients
{
    /// <inheritdoc />
    public class RequestResponseRestClient : IRequestResponseService
    {
        private readonly IHttpSender _httpSender;

        /// <summary>
        /// Constructor
        /// </summary>
        public RequestResponseRestClient(IHttpSender httpSender)
        {
            _httpSender = httpSender;
        }

        /// <inheritdoc />
        public async Task<HttpResponse> ReadResponseAsync(string requestId, CancellationToken  cancellationToken = default)
        {
            var relativeUrl = $"/Requests/{WebUtility.UrlEncode(requestId)}/Response";
            var result = await _httpSender.SendRequestAsync<AsyncHttpResponse, object>(HttpMethod.Get, relativeUrl, cancellationToken: cancellationToken);
            var response = result.Body;
            if (response == null) return null;
            FulcrumAssert.IsValidated(response, CodeLocation.AsString());
            return !response.HasCompleted ? null : response;
        }

        /// <inheritdoc />
        public async Task<HttpResponse[]> ReadWaitingResponsesAsync(string waitingRequestId, int? responseLimit, double? timeLimitInSeconds, CancellationToken cancellationToken = default)
        {
            var relativeUrl = $"/Requests/{WebUtility.UrlEncode(waitingRequestId)}/WaitingForResponses";
            var delimiter = "?";
            if (responseLimit.HasValue)
            {
                relativeUrl += $"{delimiter}responseLimit={responseLimit.Value}";
                delimiter = "&";
            }
            if (timeLimitInSeconds.HasValue)
            {
                relativeUrl += $"{delimiter}timeLimitInSeconds={timeLimitInSeconds.Value.ToString(CultureInfo.InvariantCulture)}";
            }
            var result = await _httpSender.SendRequestAsync<AsyncHttpResponse[], object>(HttpMethod.Get, relativeUrl, cancellationToken: cancellationToken);
            var responses = result.Body;
            FulcrumAssert.IsNotNull(responses, CodeLocation.AsString());
            FulcrumAssert.IsValidated(responses, CodeLocation.AsString());
            return responses.Select(r => r as HttpResponse).ToArray();
        }
    }
}