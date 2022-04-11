using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Platform.Configurations;

namespace Nexus.Link.Misc.AspNet.Sdk.Inbound.Options
{
    /// <summary>
    /// Saves the tenant specific configuration to the application context.
    /// </summary>
    public class SaveTenantConfigurationOptions : Feature, IValidatable
    {
        /// <summary>
        ///  TODO
        /// </summary>
        public ILeverServiceConfiguration ServiceConfiguration { get; set; }

        /// <inheritdoc />
        public void Validate(string errorLocation, string propertyPath = "")
        {
            if (Enabled)
            {
                FulcrumValidate.IsNotNull(ServiceConfiguration, nameof(ServiceConfiguration), errorLocation);
            }
        }
    }
}