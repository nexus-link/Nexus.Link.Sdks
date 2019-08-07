#pragma warning disable 1591
namespace Nexus.Link.Services.Implementations.BusinessApi.Startup.Configuration
{
    public class NexusCapabilityEndpoints : ConfigurationBase
    {
        public NexusCapabilityEndpoints(ConfigurationBase parent, string sectionName)
            :base(parent, sectionName)
        {
        }

        public string AppSupport => GetMandatoryValue<string>("AppSupport");

        public string BusinessEvents => GetMandatoryValue<string>("BusinessEvents");

    }
}