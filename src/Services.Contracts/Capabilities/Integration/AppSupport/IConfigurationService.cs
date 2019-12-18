using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Crud.Interfaces;

namespace Nexus.Link.Services.Contracts.Capabilities.Integration.AppSupport
{
    /// <summary>
    /// Service for getting application configurations
    /// </summary>
    public interface IConfigurationService : IRead<JToken, string>
    {
    }
}