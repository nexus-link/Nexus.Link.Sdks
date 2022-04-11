using Nexus.Link.Libraries.Core.Assert;

namespace Nexus.Link.Misc.AspNet.Sdk.Inbound.Options
{
    /// <summary>
    /// Saves the test context to the application context.
    /// </summary>
    public class SaveNexusTestContextOptions : Feature, IValidatable
    {
        /// <inheritdoc />
        public void Validate(string errorLocation, string propertyPath = "")
        {
        }
    }
}