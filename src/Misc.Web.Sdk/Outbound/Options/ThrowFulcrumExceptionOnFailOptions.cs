using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Web.Pipe;

namespace Nexus.Link.Misc.Web.Sdk.Outbound.Options
{
    /// <summary>
    /// Forward header <see cref="Constants.FulcrumCorrelationIdHeaderName"/>
    /// </summary>
    public class ThrowFulcrumExceptionOnFailOptions : Feature, IValidatable
    {
        /// <inheritdoc />
        public void Validate(string errorLocation, string propertyPath = "")
        {
        }
    }
}