using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract.Services;
using Nexus.Link.Libraries.Web.RestClientHelper;

namespace Nexus.Link.AsyncManager.Sdk.RestClients
{
    /// <inheritdoc />
    public class AsyncRequestMgmtRestClients : IAsyncRequestMgmtCapability
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public AsyncRequestMgmtRestClients(IHttpSender httpSender)
        {
            Request = new RequestRestClient(httpSender);
            RequestResponse = new RequestResponseRestClient(httpSender);
        }
        /// <inheritdoc />
        public IRequestService Request { get; }

        /// <inheritdoc />
        public IRequestResponseService RequestResponse { get; }
    }
}