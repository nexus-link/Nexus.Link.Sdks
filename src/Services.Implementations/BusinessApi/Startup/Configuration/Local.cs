#pragma warning disable 1591
namespace Nexus.Link.Services.Implementations.BusinessApi.Startup.Configuration
{
    public class Local : ConfigurationBase
    {
        public Local(ConfigurationBase parent, string sectionName)
            :base(parent, sectionName)
        {
            Authentication = new Authentication(this,"Authentication");
        }

        public Authentication Authentication { get; } 

    }
}