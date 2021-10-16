using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract.Entities;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract.Services;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Web.RestClientHelper;

namespace Nexus.Link.AsyncManager.Sdk.RestClients
{
    /// <inheritdoc />
    public class RequestResponseRestClient : IRequestResponseService
    {
        private readonly IHttpSender _httpSender;
        private readonly Tenant _tenant;

        /// <summary>
        /// Constructor
        /// </summary>
        public RequestResponseRestClient(IHttpSender httpSender) : this(FulcrumApplication.Setup.Tenant, httpSender)
        {
        }

        /// <summary>
        /// Constructor with tenant
        /// </summary>
        public RequestResponseRestClient(Tenant tenant, IHttpSender httpSender)
        {
            _tenant = tenant;
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