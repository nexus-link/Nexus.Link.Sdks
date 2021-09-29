using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract.Entities;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract.Services;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Web.RestClientHelper;

namespace Nexus.Link.AsyncManager.Sdk
{
    /// <inheritdoc />
    public class AsyncRequestMgmtRestClients : IAsyncRequestMgmtCapability
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public AsyncRequestMgmtRestClients(HttpSender httpSender)
        {
            Request = new RequestRestClient(httpSender);
            RequestResponse = new RequestResponseRestClient(httpSender);
        }
        /// <inheritdoc />
        public IRequestService Request { get; }

        /// <inheritdoc />
        public IRequestResponseService RequestResponse { get; }
    }

    /// <inheritdoc />
    public class RequestResponseRestClient : IRequestResponseService
    {
        private readonly HttpSender _httpSender;
        private readonly Tenant _tenant;

        /// <summary>
        /// Constructor
        /// </summary>
        public RequestResponseRestClient(HttpSender httpSender)
        {
            _tenant = FulcrumApplication.Setup.Tenant;
            _httpSender = httpSender;
        }

        /// <inheritdoc />
        public async Task<HttpResponse> ReadResponseAsync(string requestId, CancellationToken  cancellationToken = default)
        {
            var result = await _httpSender.SendRequestAsync<AsyncHttpResponse, object>(HttpMethod.Get, $"api/v1/Tenant/{_tenant.Organization}/{_tenant.Environment}/Requests/{requestId}/Response", cancellationToken: cancellationToken);
            var response = result.Body;
            if (response == null) return null;
            FulcrumAssert.IsValidated(response, CodeLocation.AsString());
            return !response.HasCompleted ? null : response;
        }
    }
}