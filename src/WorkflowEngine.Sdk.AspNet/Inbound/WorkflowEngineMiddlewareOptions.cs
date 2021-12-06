#if NETCOREAPP
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Web.AspNet.Pipe.Support.Options;
using Nexus.Link.WorkflowEngine.Sdk.AspNet.Inbound.Options;

namespace Nexus.Link.WorkflowEngine.Sdk.AspNet.Inbound
{
    // TODO: Move all features into a class, Feature
    public class WorkflowEngineMiddlewareOptions : IValidatable
    {
        public WorkflowEngineMiddlewareFeatures Features = new();
        /// <inheritdoc />
        public virtual void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsValidated(Features, propertyPath, nameof(Features), errorLocation);
        }
    }
}
#endif