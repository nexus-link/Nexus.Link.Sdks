using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.MemoryStorage;
using Nexus.Link.Services.Contracts.Events;
using Nexus.Link.Services.Contracts.Events.SynchronizedEntity;

namespace Nexus.Link.Services.Contracts.DataSync
{
    // TODO: This class does not belong in the Services.Contracts library, but it was put here to avoid introducing a new library with only this file.
    /// <inheritdoc cref="IDataSyncCreate{T}" />
    /// <inheritdoc cref="IDataSyncReadUpdate{T}" />
    public class DataSyncMemory<TModel> : DataSyncMemory<TModel, TModel>, IDataSyncCreate<TModel>
    {
        /// <inheritdoc />
        public DataSyncMemory(string clientName, string entityName, GetParentIdDelegate<TModel> getParentIdDelegate = null) 
            : base(clientName, entityName, getParentIdDelegate)
        {
        }
    }

    // TODO: This class does not belong in the Services.Contracts library, but it was put here to avoid introducing a new library with only this file.
    /// <inheritdoc cref="IDataSyncCreate{T}" />
    /// <inheritdoc cref="IDataSyncReadUpdate{T}" />
    public class DataSyncMemory<TModelCreate, TModel> : ManyToOneMemory<TModelCreate, TModel, string>,
        IDataSyncCreate<TModelCreate, TModel>, IDataSyncReadUpdate<TModel>, 
        IDataSyncTesting<TModel>
        where TModel : TModelCreate
    {
        protected readonly string ClientName;
        protected readonly string EntityName;

        /// <inheritdoc />
        public DataSyncMemory(string clientName, string entityName, GetParentIdDelegate<TModel> getParentIdDelegate = null)
            :base(getParentIdDelegate)
        {
            ClientName = clientName;
            EntityName = entityName;
        }

        /// <inheritdoc />
        public override async Task<string> CreateAsync(TModelCreate item, CancellationToken token = new CancellationToken())
        {
            var id = await base.CreateAsync(item, token);
            await PublishEvent(id, token);
            return id;
        }

        /// <inheritdoc />
        public override async Task UpdateAsync(string id, TModel item, CancellationToken token = new CancellationToken())
        {
            await base.UpdateAsync(id, item, token);
            await PublishEvent(id, token);
        }

        protected virtual async Task PublishEvent(string id, CancellationToken token)
        {
            var updatedEvent = new DataSyncEntityWasUpdated
            {
                Key =
                {
                    ClientName = ClientName,
                    EntityName = EntityName,
                    Value = id
                }
            };
            await updatedEvent.PublishAsync(token);
        }

        /// <inheritdoc />
        public override Task<PageEnvelope<TModel>> ReadAllWithPagingAsync(int offset, int? limit = null,
            CancellationToken token = default)
        {
            if (FulcrumApplication.IsInProductionOrProductionSimulation)
                throw new FulcrumNotImplementedException(
                    "This method is not expected to run in a production environment");
            return base.ReadAllWithPagingAsync(offset, limit, token);
        }

        /// <inheritdoc />
        public override Task<PageEnvelope<TModel>> ReadChildrenWithPagingAsync(string parentId, int offset, int? limit = null,
            CancellationToken token = default)
        {
            if (FulcrumApplication.IsInProductionOrProductionSimulation)
                throw new FulcrumNotImplementedException(
                    "This method is not expected to run in a production environment");
            return base.ReadChildrenWithPagingAsync(parentId, offset, limit, token);
        }

        /// <inheritdoc />
        public override Task DeleteAsync(string id, CancellationToken token = default)
        {
            if (FulcrumApplication.IsInProductionOrProductionSimulation)
                throw new FulcrumNotImplementedException(
                    "This method is not expected to run in a production environment");
            return base.DeleteAsync(id, token);
        }

        /// <inheritdoc />
        public override Task DeleteChildrenAsync(string masterId, CancellationToken token = default)
        {
            if (FulcrumApplication.IsInProductionOrProductionSimulation)
                throw new FulcrumNotImplementedException(
                    "This method is not expected to run in a production environment");
            return base.DeleteChildrenAsync(masterId, token);
        }

        /// <inheritdoc />
        public override Task DeleteAllAsync(CancellationToken token = default)
        {
            if (FulcrumApplication.IsInProductionOrProductionSimulation)
                throw new FulcrumNotImplementedException(
                    "This method is not expected to run in a production environment");
            return base.DeleteAllAsync(token);
        }
    }
}
