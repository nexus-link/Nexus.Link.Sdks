using Nexus.Link.Libraries.Core.Assert;

namespace Nexus.Link.Misc.AspNet.Sdk.Inbound.Options
{
    /// <summary>
    /// Saves the correlation id to the application context.
    /// </summary>
    public class SaveCorrelationIdOptions : Feature, IValidatable
    {
        /// <inheritdoc />
        public void Validate(string errorLocation, string propertyPath = "")
        {
        }
    }
}