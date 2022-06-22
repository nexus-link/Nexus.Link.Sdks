using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Context;
using Nexus.Link.Libraries.Web.Pipe;

namespace Misc.Web.Sdk.Outbound.Options
{
    /// <summary>
    /// Forward header <see cref="Constants.NexusUserAuthorizationHeaderName"/>
    /// </summary>
    public class ForwardNexusUserAuthorizationOptions : Feature, IValidatable
    {
        public IContextValueProvider ContextValueProvider { get; }

        public ForwardNexusUserAuthorizationOptions()
        {
            ContextValueProvider = new AsyncLocalContextValueProvider();
        }

        /// <inheritdoc />
        public void Validate(string errorLocation, string propertyPath = "")
        {
        }
    }
}