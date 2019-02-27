using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.MultiTenant.Model;

namespace Nexus.Link.Logger.Sdk.RestClients
{
    /// <summary>
    /// The way that we actually log things.
    /// </summary>
    public interface ILogClient
    {
        /// <summary>
        /// Log <paramref name="logs"/>.
        /// </summary>
        /// <returns></returns>
        Task LogAsync(Tenant tenant, params LogMessage[] logs);
    }
}
