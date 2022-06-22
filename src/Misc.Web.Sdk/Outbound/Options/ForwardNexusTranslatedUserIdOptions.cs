using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Context;
using Nexus.Link.Libraries.Web.Pipe;

namespace Misc.Web.Sdk.Outbound.Options
{
    /// <summary>
    /// Forward header <see cref="Constants.NexusTranslatedUserIdHeaderName"/>
    /// </summary>
    public class ForwardNexusTranslatedUserIdOptions : Feature, IValidatable
    {
        public IContextValueProvider ContextValueProvider { get; }

        public ForwardNexusTranslatedUserIdOptions()
        {
            ContextValueProvider = new AsyncLocalContextValueProvider();
        }

        /// <inheritdoc />
        public void Validate(string errorLocation, string propertyPath = "")
        {
        }
    }
}