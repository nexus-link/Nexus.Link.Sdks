using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Crud.MemoryStorage;
using Nexus.Link.Services.Contracts.DataSync;
using Nexus.Link.Services.Contracts.Events.SynchronizedEntity;
using Nexus.Link.Services.Implementations.Adapter.Events;

namespace Nexus.Link.Services.Implementations.Adapter.DataSync
{
    /// <inheritdoc cref="IDataSyncCreate{T}" />
    /// <inheritdoc cref="IDataSyncReadUpdate{T}" />
    public class DataSyncMemoryMock<TModel> : IDataSyncCreate<TModel>, IDataSyncReadUpdate<TModel>
    {
        private readonly string _clientName;
        private readonly string _entityName;
        private readonly CrudMemory<TModel, string> _repository = new CrudMemory<TModel, string>();

        /// <inheritdoc />
        public DataSyncMemoryMock(string clientName, string entityName)
        {
            _clientName = clientName;
            _entityName = entityName;
        }

        /// <inheritdoc />
        public async Task<string> CreateAsync(TModel item, CancellationToken token = new CancellationToken())
        {
            var id = await _repository.CreateAsync(item, token);
            await PublishEvent(id, token);
            return id;
        }

        /// <inheritdoc />
        public async Task<TModel> ReadAsync(string id, CancellationToken token = new CancellationToken())
        {
            var item = await _repository.ReadAsync(id, token);
            return item;
        }

        /// <inheritdoc />
        public async Task UpdateAsync(string id, TModel item, CancellationToken token = new CancellationToken())
        {
            await _repository.UpdateAsync(id, item, token);
            await PublishEvent(id, token);
        }

        private async Task PublishEvent(string id, CancellationToken token)
        {
            var updatedEvent = new SynchronizedEntityWasUpdated
            {
                ClientName = _clientName,
                EntityName = _entityName,
                Id = id
            };
            await updatedEvent.PublishAsync(token);
        }
    }
}
