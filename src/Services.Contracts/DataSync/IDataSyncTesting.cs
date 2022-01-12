using Nexus.Link.Libraries.Crud.Interfaces;

namespace Nexus.Link.Services.Contracts.DataSync
{
    /// <summary>
    /// Additional methods needed for testing data sync.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IDataSyncTesting<T> : IReadAllWithPaging<T, string>, IDelete<string>, IReadChildrenWithPaging<T, string>, IDeleteAll
    {
    }
}