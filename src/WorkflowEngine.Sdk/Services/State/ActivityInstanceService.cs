using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Execution;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Services;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Extensions.State;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Services.State
{
    public class ActivityInstanceService : IActivityInstanceService
    {
        private readonly IRuntimeTables _runtimeTables;
        private readonly IAsyncRequestMgmtCapability _requestMgmtCapability;

        public ActivityInstanceService(IRuntimeTables runtimeTables, IAsyncRequestMgmtCapability requestMgmtCapability)
        {
            _runtimeTables = runtimeTables;
            _requestMgmtCapability = requestMgmtCapability;
        }

        /// <inheritdoc />
        public async Task<ActivityInstance> CreateWithSpecifiedIdAndReturnAsync(string id, ActivityInstanceCreate item, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));

            var idAsGuid = id.ToGuid();
            var recordCreate = new ActivityInstanceRecordCreate().From(item);
            var result = await _runtimeTables.ActivityInstance.CreateWithSpecifiedIdAndReturnAsync(idAsGuid, recordCreate, cancellationToken);
            FulcrumAssert.IsValidated(result, CodeLocation.AsString());
            return new ActivityInstance().From(result);
        }

        /// <inheritdoc />
        public async Task SetContextAsync(string id, string key, JToken content,
            CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));
            InternalContract.RequireNotNullOrWhiteSpace(key, nameof(key));
            InternalContract.RequireNotNull(content, nameof(content));
            var idAsGuid = id.ToGuid();

            var tryCount = 0;
            ActivityInstanceRecord record;
            while (true)
            {
                tryCount++;
                record = await _runtimeTables.ActivityInstance.ReadAsync(idAsGuid, cancellationToken);
                if (record == null) throw new FulcrumNotFoundException($"No ActivityInstance record with id = {id}");

                var activity = new ActivityInstance().From(record);
                activity.ContextDictionary[key] = content;

                record = new ActivityInstanceRecord().From(activity);
                try
                {
                    await _runtimeTables.ActivityInstance.UpdateAndReturnAsync(idAsGuid, record, cancellationToken);
                    break;
                }
                catch (FulcrumConflictException)
                {
                    if (tryCount >= 3) throw;
                    // This is OK, try again
                }
            }

            await _requestMgmtCapability.Request.RetryAsync(record.WorkflowInstanceId.ToGuidString(), cancellationToken);
        }

        /// <inheritdoc />
        public async Task<ActivityInstance> UpdateAndReturnAsync(string id, ActivityInstance item, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));

            var idAsGuid = id.ToGuid();

            var record = new ActivityInstanceRecord().From(item);
            var result = await _runtimeTables.ActivityInstance.UpdateAndReturnAsync(idAsGuid, record, cancellationToken);
            FulcrumAssert.IsValidated(result, CodeLocation.AsString());
            return new ActivityInstance().From(result);
        }

        /// <inheritdoc />
        public async Task<ActivityInstance> ReadAsync(string id, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));
            var idAsGuid = id.ToGuid();

            var record = await _runtimeTables.ActivityInstance.ReadAsync(idAsGuid, cancellationToken);
            if (record == null) return null;

            var result = new ActivityInstance().From(record);
            FulcrumAssert.IsNotNull(result, CodeLocation.AsString());
            FulcrumAssert.IsValidated(result, CodeLocation.AsString());
            return result;
        }
    }
}