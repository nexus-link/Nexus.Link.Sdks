using Nexus.Link.Libraries.Core.Assert;

namespace Nexus.Link.Misc.AspNet.Sdk.Inbound.Options
{
    /// <summary>
    /// Saves the execution id to the context.
    /// </summary>
    public class SaveExecutionIdOptions : Feature, IValidatable
    {
        /// <inheritdoc />
        public void Validate(string errorLocation, string propertyPath = "")
        {
        }
    }
}