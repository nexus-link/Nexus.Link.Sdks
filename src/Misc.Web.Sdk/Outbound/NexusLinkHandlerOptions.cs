using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Misc.Web.Sdk.Outbound.Options;

namespace Nexus.Link.Misc.Web.Sdk.Outbound
{
    /// <summary>
    /// Options that controls <see cref="NexusLinkHandler"/>
    /// </summary>
    public class NexusLinkHandlerOptions : IValidatable
    {
        /// <summary>
        /// The features that can be controlled.
        /// </summary>
        public HandlerFeatures Features = new HandlerFeatures();

        /// <inheritdoc />
        public virtual void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsValidated(Features, propertyPath, nameof(Features), errorLocation);
        }
    }
}