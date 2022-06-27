using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Web.Pipe;

namespace Nexus.Link.Misc.Web.Sdk.OutboundHandlers.Options
{
    /// <summary>
    /// Forward header <see cref="Constants.FulcrumCorrelationIdHeaderName"/>
    /// </summary>
    public class RerouteAsynchronousRequestsOptions : Feature, IValidatable
    {
        /// <summary>
        /// The async request management capability that can take care of asynchronous requests.
        /// </summary>
        public IAsyncRequestMgmtCapability AsyncRequestMgmtCapability { get; set; }

        /// <inheritdoc />
        public void Validate(string errorLocation, string propertyPath = "")
        {
        }
    }
}