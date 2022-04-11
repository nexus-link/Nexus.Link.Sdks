using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract.Services;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Web.AspNet.Pipe.Support.Options;

namespace Nexus.Link.WorkflowEngine.Sdk.AspNet.Inbound.Options
{
    /// <summary>
    /// Options for redirecting requests to an asynchronous request-responses capability
    /// </summary>
    public class RedirectAsynchronousRequestsOptions : Feature, IValidatable
    {
        /// <inheritdoc />
        public void Validate(string errorLocation, string propertyPath = "")
        {
            if (Enabled)
            {
                FulcrumValidate.IsNotNull(RequestService, nameof(RequestService), errorLocation);
            }
        }

        /// <summary>
        /// The async request service that we should redirect requests that need to be asynchronous to.
        /// </summary>
        public IRequestService RequestService { get; set; }


    }
}