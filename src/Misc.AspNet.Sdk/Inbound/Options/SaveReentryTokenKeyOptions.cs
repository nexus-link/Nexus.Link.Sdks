using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Web.Pipe;

namespace Nexus.Link.Misc.AspNet.Sdk.Inbound.Options
{
    /// <summary>
    /// If the header <see cref="Constants.ReentryAuthenticationHeaderName"/> is set and associated with this token, ignore that the token has expired.
    /// </summary>
    public class SaveReentryAuthenticationOptions : Feature, IValidatable
    {
        /// <inheritdoc />
        public void Validate(string errorLocation, string propertyPath = "")
        {
        }
    }
}