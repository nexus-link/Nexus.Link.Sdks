using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Web.Pipe;

namespace Misc.Web.Sdk.Outbound.Options
{
    /// <summary>
    /// Logs the request and the response
    /// </summary>
    public class LogRequestAndResponseOptions : Feature, IValidatable
    {
        /// <inheritdoc />
        public void Validate(string errorLocation, string propertyPath = "")
        {
        }
    }
}