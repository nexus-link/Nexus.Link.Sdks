using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Web.Pipe;

namespace Nexus.Link.Misc.Web.Sdk.Outbound.Options
{
    /// <summary>
    /// Forward header <see cref="Constants.NexusTestContextHeaderName"/>
    /// </summary>
    public class ForwardNexusTestContextOptions : Feature, IValidatable
    {
        /// <inheritdoc />
        public void Validate(string errorLocation, string propertyPath = "")
        {
        }
    }
}