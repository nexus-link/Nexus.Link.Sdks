using Nexus.Link.Libraries.Crud.Interfaces;

namespace Nexus.Link.Services.Contracts.DataSync
{
    /// <summary>
    /// The optional Create method for data sync.
    /// </summary>
    public interface IDataSyncCreate<T> : IDataSyncCreate<T, T>, ICreate<T, string>
    {
    }
    /// <summary>
    /// The optional Create method for data sync.
    /// </summary>
    public interface IDataSyncCreate<in TModelCreate, TModel> : ICreate<TModelCreate, TModel, string> where TModel : TModelCreate
    {
    }
}