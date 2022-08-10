using System;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Web.Pipe;

namespace Nexus.Link.WorkflowEngine.Sdk.AspNet.Inbound.Options
{
    [Obsolete("Please use Nexus.Link.Misc.AspNet.Sdk.Inbound.NexusLinkMiddleware. Obsolete since 2022-04-07.")]
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