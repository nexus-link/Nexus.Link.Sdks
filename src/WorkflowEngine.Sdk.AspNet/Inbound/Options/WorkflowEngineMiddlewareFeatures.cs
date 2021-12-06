#if NETCOREAPP
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Web.AspNet.Pipe.Support.Options;
using Nexus.Link.Libraries.Web.Pipe;

namespace Nexus.Link.WorkflowEngine.Sdk.AspNet.Inbound.Options
{
    // TODO: Move all features into a class, Feature
    public class WorkflowEngineMiddlewareFeatures : IValidatable
    {
        /// 

        /// <summary>
        /// This feature gets the first found <see cref="Constants.ExecutionIdHeaderName"/> header from the request and saves it to the <see cref="FulcrumApplication.Context"/>.
        /// </summary>
        public RedirectAsynchronousRequestsOptions RedirectAsynchronousRequests{ get; } = new();

        /// <inheritdoc />
        public virtual void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsValidated(RedirectAsynchronousRequests, propertyPath, nameof(RedirectAsynchronousRequests), errorLocation);
        }
    }
}
#endif