using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Context;
using Nexus.Link.Libraries.Web.Pipe;

namespace Nexus.Link.Misc.Web.Sdk.OutboundHandlers.Options
{
    /// <summary>
    /// Forward header <see cref="Constants.NexusUserAuthorizationHeaderName"/>
    /// </summary>
    public class ForwardNexusUserAuthorizationOptions : Feature, IValidatable
    {
        /// <summary>
        /// The context value provider where the header value is taken from
        /// </summary>
        public IContextValueProvider ContextValueProvider { get; } = new AsyncLocalContextValueProvider();

        /// <inheritdoc />
        public void Validate(string errorLocation, string propertyPath = "")
        {
        }
    }
}