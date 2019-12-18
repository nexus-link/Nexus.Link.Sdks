using Nexus.Link.Libraries.Crud.Interfaces;

namespace Nexus.Link.Services.Contracts.DataSync
{
    /// <summary>
    /// The most basic methods for data sync; Read and Update
    /// </summary>
    public interface IDataSyncReadUpdate<T> : IRead<T, string>, IUpdate<T, string>
    {
    }
}