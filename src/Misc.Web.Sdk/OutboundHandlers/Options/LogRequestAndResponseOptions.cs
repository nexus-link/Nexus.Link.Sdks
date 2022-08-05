using Nexus.Link.Libraries.Core.Assert;

namespace Nexus.Link.Misc.Web.Sdk.OutboundHandlers.Options
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