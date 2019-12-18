using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Platform.DataSyncEngine;

namespace Nexus.Link.DatasyncEngine.Sdk.RestClients
{
    internal interface ISynchronizedEntityClient
    {
        Task SynchronizedEntityAssociatedAsync(KeyAssociations associations);
    }
}