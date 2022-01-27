using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract.Services;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Web.AspNet.Pipe.Support.Options;

namespace Nexus.Link.WorkflowEngine.Sdk.AspNet.Inbound.Options
{
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

        public IRequestService RequestService { get; set; }
    }
}