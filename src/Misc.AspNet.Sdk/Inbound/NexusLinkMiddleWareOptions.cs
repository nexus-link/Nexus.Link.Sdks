#if NETCOREAPP
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Misc.AspNet.Sdk.Inbound.Options;

namespace Nexus.Link.Misc.AspNet.Sdk.Inbound
{
    /// <summary>
    /// Options that controls <see cref="NexusLinkMiddleware"/>
    /// </summary>
    public class NexusLinkMiddlewareOptions : IValidatable
    {
        /// <summary>
        /// The features that can be controlled.
        /// </summary>
        public MiddlewareFeatures Features = new MiddlewareFeatures();
        /// <inheritdoc />
        public virtual void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsValidated(Features, propertyPath, nameof(Features), errorLocation);
        }
    }
}
#endif