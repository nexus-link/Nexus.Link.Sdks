using Nexus.Link.Libraries.Core.Assert;

namespace Nexus.Link.Misc.AspNet.Sdk.Inbound.Options
{
    /// <summary>
    /// Logs all requests and their responses
    /// </summary>
    public class LogRequestAndResponseOptions : Feature, IValidatable
    {
        /// <inheritdoc />
        public void Validate(string errorLocation, string propertyPath = "")
        {
        }
    }
}