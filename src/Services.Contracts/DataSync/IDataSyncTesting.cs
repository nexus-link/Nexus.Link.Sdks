using Nexus.Link.Libraries.Crud.Interfaces;

namespace Nexus.Link.Services.Contracts.DataSync
{
    public interface IDataSyncTesting<T> : IReadAllWithPaging<T, string>, IDelete<string>, IReadChildrenWithPaging<T, string>, IDeleteAll
    {
    }
}