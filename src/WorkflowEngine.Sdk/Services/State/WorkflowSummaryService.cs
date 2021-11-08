using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nexus.Link.AsyncManager.Sdk;
using Nexus.Link.AsyncManager.Sdk.RestClients;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.Configuration;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.State;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.State;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Crud.Helpers;
using Nexus.Link.Libraries.Crud.Model;
using Nexus.Link.WorkflowEngine.Sdk.Extensions.Configuration;
using Nexus.Link.WorkflowEngine.Sdk.Extensions.State;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Services.State
{

    public class WorkflowSummaryService : IWorkflowSummaryService
    {
        private readonly IConfigurationTables _configurationTables;
        private readonly IRuntimeTables _runtimeTables;
        private readonly IAsyncRequestClient _asyncRequestClient;

        public WorkflowSummaryService(IConfigurationTables configurationTables, IRuntimeTables runtimeTables,
            IAsyncRequestMgmtCapability requestMgmtCapability)
        {
            _configurationTables = configurationTables;
            _runtimeTables = runtimeTables;
            _asyncRequestClient = new AsyncRequestClient(requestMgmtCapability);
        }

        public async Task<WorkflowSummary> GetSummaryAsync(string instanceId,
            CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(instanceId, nameof(instanceId));

            var instanceIdAsGuid = MapperHelper.MapToType<Guid, string>(instanceId);
            var instance = await _runtimeTables.WorkflowInstance.ReadAsync(instanceIdAsGuid, cancellationToken);
            if (instance == null) return null;

            var version = await _configurationTables.WorkflowVersion.ReadAsync(instance.WorkflowVersionId, cancellationToken);
            FulcrumAssert.IsNotNull(version, CodeLocation.AsString());
            var form = await _configurationTables.WorkflowForm.ReadAsync(version.WorkflowFormId, cancellationToken);
            FulcrumAssert.IsNotNull(form, CodeLocation.AsString());

            return await GetSummaryAsync(form, version, instance, cancellationToken);
        }

        public async Task<WorkflowSummary> GetSummaryAsync(string formId, int majorVersion, string instanceId,
            CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(formId, nameof(formId));
            InternalContract.RequireGreaterThanOrEqualTo(0, majorVersion, nameof(majorVersion));
            InternalContract.RequireNotNullOrWhiteSpace(instanceId, nameof(instanceId));

            // Form
            var formIdAsGuid = MapperHelper.MapToType<Guid, string>(formId);
            var form = await _configurationTables.WorkflowForm.ReadAsync(formIdAsGuid, cancellationToken);

            // Version
            WorkflowVersionRecord version = null;
            if (form != null)
            {
                version = await _configurationTables.WorkflowVersion.ReadByFormAndMajorAsync(formIdAsGuid, majorVersion,
                    cancellationToken);
            }

            // Instance
            var instanceIdAsGuid = MapperHelper.MapToType<Guid, string>(instanceId);
            var instance = await _runtimeTables.WorkflowInstance.ReadAsync(instanceIdAsGuid, cancellationToken);

            return await GetSummaryAsync(form, version, instance, cancellationToken);
        }

        private async Task<WorkflowSummary> GetSummaryAsync(WorkflowFormRecord form, WorkflowVersionRecord version, WorkflowInstanceRecord instance,
            CancellationToken cancellationToken = default)
        {
            // Activities
            var (activityForms, activityVersions, activityInstances) =
                await ReadAllActivitiesAndUpdateResponsesAsync(form?.Id, version?.Id, instance?.Id, cancellationToken);

            var activityTree = BuildActivityTree(null, activityForms, activityVersions, activityInstances);
            activityTree.Sort(PositionSort);

            var workflowSummary = new WorkflowSummary
            {
                Instance = instance == null ? null : new WorkflowInstance().From(instance),
                Form = form == null ? null : new WorkflowForm().From(form),
                Version = version == null ? null : new WorkflowVersion().From(version),
                ActivityForms = activityForms,
                ActivityVersions = activityVersions,
                ActivityInstances = activityInstances,
                ReferredActivities = BuildReferred(activityForms, activityVersions, activityInstances),
                ActivityTree = activityTree
            };

            return workflowSummary;
        }

        private static int PositionSort(ActivitySummary x, ActivitySummary y)
        {
            return x.Version.Position.CompareTo(y.Version.Position);
        }

        private List<ActivitySummary> BuildReferred(Dictionary<string, ActivityForm> activityForms,
            Dictionary<string, ActivityVersion> activityVersions,
            Dictionary<string, ActivityInstance> activityInstances)
        {
            var activities = new List<ActivitySummary>();
            foreach (var entry in activityInstances)
            {
                var instance = entry.Value;
                var version = activityVersions[instance.ActivityVersionId];
                var form = activityForms[version.ActivityFormId];
                var activity = new ActivitySummary
                {
                    Instance = instance,
                    Version = version,
                    Form = form
                };
                activity.Children = BuildActivityTree(activity, activityForms, activityVersions, activityInstances);
                activities.Add(activity);
            }

            return activities;
        }

        private List<ActivitySummary> BuildActivityTree(ActivitySummary parent, Dictionary<string, ActivityForm> activityForms,
            Dictionary<string, ActivityVersion> activityVersions,
            Dictionary<string, ActivityInstance> activityInstances)
        {
            var activities = new List<ActivitySummary>();
            foreach (var entry in activityInstances.Where(x =>
                x.Value.ParentActivityInstanceId == parent?.Instance.Id))
            {
                var instance = entry.Value;
                var version = activityVersions[instance.ActivityVersionId];
                var form = activityForms[version.ActivityFormId];
                var activity = new ActivitySummary
                {
                    Instance = instance,
                    Version = version,
                    Form = form
                };
                activity.Children = BuildActivityTree(activity, activityForms, activityVersions, activityInstances);
                activities.Add(activity);
            }

            return activities;
        }

        private async Task<(Dictionary<string, ActivityForm> activityForms,
                Dictionary<string, ActivityVersion> activityVersions, Dictionary<string, ActivityInstance>
                activityInstances)>
            ReadAllActivitiesAndUpdateResponsesAsync(Guid? formId, Guid? versionId, Guid? instanceId, CancellationToken cancellationToken)
        {
            // ActivityForms
            Dictionary<string, ActivityForm> activityForms;
            if (formId == null)
            {
                activityForms = new Dictionary<string, ActivityForm>();
            }
            else
            {
                var activityFormsList = await _configurationTables.ActivityForm.SearchAsync(
                    new SearchDetails<ActivityFormRecord>(new ActivityFormRecordSearch { WorkflowFormId = formId.Value }), 0,
                    int.MaxValue, cancellationToken);
                activityForms = activityFormsList.Data.ToDictionary(x => MapperHelper.MapToType<string, Guid>(x.Id), x => new ActivityForm().From(x));
            }
            // ActivityVersions
            Dictionary<string, ActivityVersion> activityVersions;
            if (versionId == null)
            {
                activityVersions = new Dictionary<string, ActivityVersion>();
            }
            else
            {
                var activityVersionsList = await _configurationTables.ActivityVersion.SearchAsync(
                    new SearchDetails<ActivityVersionRecord>(new ActivityVersionRecordSearch
                    { WorkflowVersionId = versionId.Value }), 0, int.MaxValue, cancellationToken);
                activityVersions = activityVersionsList.Data.ToDictionary(x => MapperHelper.MapToType<string, Guid>(x.Id), x => new ActivityVersion().From(x));
            }

            // ActivityInstances
            Dictionary<string, ActivityInstance> activityInstances;
            if (instanceId == null)
            {
                activityInstances = new Dictionary<string, ActivityInstance>();
            }
            else
            {

                var activityInstancesList = await _runtimeTables.ActivityInstance.SearchAsync(
                    new SearchDetails<ActivityInstanceRecord>(new ActivityInstanceRecordSearch
                    { WorkflowInstanceId = instanceId.Value }), 0, int.MaxValue, cancellationToken);

                var tasks = new List<Task<ActivityInstanceRecord>>();
                foreach (var activityInstanceRecord in activityInstancesList.Data)
                {
                    var task = HasCompleted(activityInstanceRecord) || string.IsNullOrWhiteSpace(activityInstanceRecord.AsyncRequestId)
                        ? Task.FromResult(activityInstanceRecord)
                        : TryGetResponseAsync(activityInstanceRecord, cancellationToken);
                    tasks.Add(task);
                }
                var activityInstancesRecords = new List<ActivityInstanceRecord>();
                foreach (var task in tasks)
                {
                    var activityInstanceRecord = await task;
                    activityInstancesRecords.Add(activityInstanceRecord);
                }

                activityInstances = activityInstancesRecords
                    .ToDictionary(x => MapperHelper.MapToType<string, Guid>(x.Id), x => new ActivityInstance().From(x));
            }

            return (activityForms, activityVersions, activityInstances);
        }

        private async Task<ActivityInstanceRecord> TryGetResponseAsync(ActivityInstanceRecord activityInstanceRecord, CancellationToken cancellationToken)
        {
            InternalContract.Require(!HasCompleted(activityInstanceRecord), "The activity instance must not be completed.");
            var retries = 0;
            while (true)
            {
                if (HasCompleted(activityInstanceRecord)) return activityInstanceRecord;
                var response = await _asyncRequestClient.GetFinalResponseAsync(activityInstanceRecord.AsyncRequestId,
                    cancellationToken);
                if (response == null || !response.HasCompleted) return activityInstanceRecord;

                if (response.Exception?.Name == null)
                {
                    activityInstanceRecord.State = ActivityStateEnum.Success.ToString();
                    activityInstanceRecord.FinishedAt = DateTimeOffset.UtcNow;
                    activityInstanceRecord.ResultAsJson = response.Content;
                }
                else
                {
                    activityInstanceRecord.State = ActivityStateEnum.Failed.ToString();
                    activityInstanceRecord.FinishedAt = DateTimeOffset.UtcNow;
                    activityInstanceRecord.ExceptionCategory = ActivityExceptionCategoryEnum.Technical.ToString();
                    activityInstanceRecord.ExceptionTechnicalMessage =
                        $"A remote method returned an exception with the name {response.Exception.Name} and message: {response.Exception.Message}";
                    activityInstanceRecord.ExceptionFriendlyMessage =
                        $"A remote method failed with the following message: {response.Exception.Message}";
                }

                try
                {
                    var result = await _runtimeTables.ActivityInstance.UpdateAndReturnAsync(activityInstanceRecord.Id, activityInstanceRecord, cancellationToken);
                    Log.LogVerbose($"ActivityInstance.UpdateAndReturnAsync(): {JsonConvert.SerializeObject(result)}");
                    return result;
                }
                catch (FulcrumConflictException)
                {
                    Log.LogVerbose($"ActivityInstance.UpdateAndReturnAsync(): {nameof(FulcrumConflictException)}");
                    // Concurrency problem, try again after short pause.
                    retries++;
                    if (retries > 5) throw;
                    Log.LogVerbose($"ActivityInstance.UpdateAndReturnAsync(): {nameof(FulcrumConflictException)} Retry");
                    await Task.Delay(TimeSpan.FromMilliseconds(10), cancellationToken);
                    activityInstanceRecord = await _runtimeTables.ActivityInstance.ReadAsync(activityInstanceRecord.Id, cancellationToken);
                }
            }
        }

        private static bool HasCompleted(ActivityInstanceRecord activityInstanceRecord)
        {
            return activityInstanceRecord.FinishedAt.HasValue;
        }
    }
}