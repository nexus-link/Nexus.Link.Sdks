using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Helpers;
using Nexus.Link.Libraries.Crud.Model;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Support;

namespace Nexus.Link.WorkflowEngine.Sdk.Services
{
    public class TransitionService : ITransitionService
    {
        private readonly IConfigurationTables _configurationTables;

        public TransitionService(IConfigurationTables configurationTables)
        {
            _configurationTables = configurationTables;
        }

        /// <inheritdoc />
        public async Task<string> CreateChildAsync(string workflowVersionId, TransitionCreate item,
            CancellationToken cancellationToken = new CancellationToken())
        {
            InternalContract.RequireNotNullOrWhiteSpace(workflowVersionId, nameof(workflowVersionId));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));

            var recordCreate = new TransitionRecordCreate().From(item);
            await VerifyUnique(recordCreate, cancellationToken);
            var idAsGuid = await _configurationTables.Transition.CreateAsync(recordCreate, cancellationToken);
            var id = MapperHelper.MapToType<string, Guid>(idAsGuid);
            return id;
        }

        /// <inheritdoc />
        public async Task<Transition> FindUniqueAsync(TransitionUnique item, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));

            var searchRecord = new TransitionRecordUnique().From(item);
            var record = await _configurationTables.Transition.FindUniqueAsync(
                new SearchDetails<TransitionRecord>(searchRecord), cancellationToken);
            if (record == null) return null;

            var result = new Transition().From(record);
            FulcrumAssert.IsNotNull(result, CodeLocation.AsString());
            FulcrumAssert.IsValidated(result, CodeLocation.AsString());
            return result;
        }

        private async Task VerifyUnique(TransitionRecordUnique searchRecord, CancellationToken cancellationToken)
        {
            var page = await _configurationTables.Transition.SearchAsync(new SearchDetails<TransitionRecord>(searchRecord), 0, 1, cancellationToken);
            if (page.PageInfo.Returned != 0)
            {
                throw new FulcrumConflictException(
                    $"A transition already exists from {searchRecord.FromActivityVersionId} to {searchRecord.ToActivityVersionId} for workflow version {searchRecord.WorkflowVersionId}.");
            }
        }

        /// <inheritdoc />
        public async Task<PageEnvelope<Transition>> ReadChildrenWithPagingAsync(string workflowVersionId, int offset, int? limit = null,
            CancellationToken token = new CancellationToken())
        {
            var parentIdAsGuid = MapperHelper.MapToType<Guid, string>(workflowVersionId);
            var page = await _configurationTables.Transition.ReadChildrenWithPagingAsync(parentIdAsGuid, offset, limit,
                token);
            var data = page.Data.Select(record => new Transition().From(record)).ToArray();
            FulcrumAssert.IsValidated(data, CodeLocation.AsString());
            return new PageEnvelope<Transition>(page.PageInfo, data);
        }
    }
}