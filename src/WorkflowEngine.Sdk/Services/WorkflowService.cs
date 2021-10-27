using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nexus.Link.AsyncManager.Sdk;
using Nexus.Link.AsyncManager.Sdk.RestClients;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.Runtime;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Crud.Helpers;
using Nexus.Link.Libraries.Crud.Model;
using Nexus.Link.WorkflowEngine.Sdk.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Support;

namespace Nexus.Link.WorkflowEngine.Sdk.Services
{

    public class WorkflowService : IWorkflowService
    {
        private readonly IConfigurationTables _configurationTables;
        private readonly IRuntimeTables _runtimeTables;
        private readonly AsyncRequestClient _asyncRequestClient;

        public WorkflowService(IConfigurationTables configurationTables, IRuntimeTables runtimeTables,
            IAsyncRequestMgmtCapability asyncRequestMgmtCapability)
        {
            _configurationTables = configurationTables;
            _runtimeTables = runtimeTables;
            _asyncRequestClient = new AsyncRequestClient(asyncRequestMgmtCapability);
        }

        public async Task<WorkflowSummary> ReadAsync(string id, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));

            var idAsGuid = MapperHelper.MapToType<Guid, string>(id);
            var instance = await _runtimeTables.WorkflowInstance.ReadAsync(idAsGuid, cancellationToken);
            if (instance == null) return null;
            var version =
                await _configurationTables.WorkflowVersion.ReadAsync(instance.WorkflowVersionId, cancellationToken);
            var form = await _configurationTables.WorkflowForm.ReadAsync(version.WorkflowFormId, cancellationToken);

            var workflow = new WorkflowSummary
            {
                Instance = new WorkflowInstance().From(instance),
                Form = new WorkflowForm().From(form),
                Version = new WorkflowVersion().From(version)
            };

            var (activityForms, activityVersions, activityInstances) =
                await ReadAllActivitiesAndUpdateResponsesAsync(form.Id, version.Id, instance.Id, cancellationToken);
            workflow.ReferredActivities = BuildReferred(activityForms, activityVersions, activityInstances);
            var activityTree = BuildActivityTree(null, activityForms, activityVersions, activityInstances);
            activityTree.Sort(PositionSort);
            workflow.ActivityTree = activityTree;
            workflow.NotReferredActivities = BuildNotReferred(activityForms, activityVersions, activityInstances);

            return workflow;
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

        private List<ActivitySummary> BuildNotReferred(Dictionary<string, ActivityForm> activityForms,
            Dictionary<string, ActivityVersion> activityVersions,
            Dictionary<string, ActivityInstance> activityInstances)
        {
            var activities = new List<ActivitySummary>();
            foreach (var (key, form) in activityForms)
            {
                var version = activityVersions.Values.FirstOrDefault(v => v.ActivityFormId == form.Id);
                FulcrumAssert.IsNotNull(version, CodeLocation.AsString());
                var instance = activityInstances.Values.FirstOrDefault(i => i.ActivityVersionId == version!.Id);
                if (instance != null) continue;
                var activity = new ActivitySummary
                {
                    Instance = null,
                    Version = version,
                    Form = form
                };
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
                x.Value.ParentActivityInstanceId?.ToString() == parent?.Instance.Id))
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
            ReadAllActivitiesAndUpdateResponsesAsync(Guid formId, Guid versionId, Guid instanceId, CancellationToken cancellationToken)
        {
            var activityFormsList = await _configurationTables.ActivityForm.SearchAsync(
                new SearchDetails<ActivityFormRecord>(new ActivityFormRecordSearch { WorkflowFormId = formId }), 0,
                int.MaxValue, cancellationToken);
            var activityForms = activityFormsList.Data.ToDictionary(x => MapperHelper.MapToType<string, Guid>(x.Id), x => new ActivityForm().From(x));

            var activityVersionsList = await _configurationTables.ActivityVersion.SearchAsync(
                new SearchDetails<ActivityVersionRecord>(new ActivityVersionRecordSearch
                { WorkflowVersionId = versionId }), 0, int.MaxValue, cancellationToken);
            var activityVersions = activityVersionsList.Data.ToDictionary(x => MapperHelper.MapToType<string, Guid>(x.Id), x => new ActivityVersion().From(x));

            var activityInstancesList = await _runtimeTables.ActivityInstance.SearchAsync(
                new SearchDetails<ActivityInstanceRecord>(new ActivityInstanceRecordSearch
                { WorkflowInstanceId = instanceId }), 0, int.MaxValue, cancellationToken);
            
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

            var activityInstances = activityInstancesRecords
                .ToDictionary(x => MapperHelper.MapToType<string, Guid>(x.Id), x => new ActivityInstance().From(x));

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
